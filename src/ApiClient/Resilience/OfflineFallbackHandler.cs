using System.Net;
using System.Net.Http.Headers;

namespace SSW.Rewards.ApiClient.Resilience;

/// <summary>
/// Makes every GET work offline by default: a successful response is stored in
/// <see cref="IApiResponseCache"/>, and when the same GET later fails without reaching the API
/// (no network, timeout, open circuit) the stored response is returned instead of the error.
/// <para>
/// Only network failures fall back. An HTTP error the API returned (401, 404, 500) passes through,
/// so a 401 still ends the session. Writes are never stored or replayed.
/// </para>
/// Pages that show cached data before the network (<c>PagedListSource</c>) still do: this is the
/// safety net for the ones that don't.
/// </summary>
public sealed class OfflineFallbackHandler(IApiResponseCache cache) : DelegatingHandler
{
    /// <summary>Set on a replayed response, for callers that want to know the data is stale.</summary>
    public const string ReplayedHeader = "X-SSW-Rewards-Offline-Replay";

    /// <summary>How long a stored response can be replayed. Sign-out wipes it sooner.</summary>
    public static readonly TimeSpan Expiry = TimeSpan.FromDays(30);

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (request.Method != HttpMethod.Get || request.RequestUri is null)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        var key = KeyFor(request.RequestUri);

        // Captured before sending: a sign-out while the request is in flight voids its write.
        var generation = cache.Generation;

        HttpResponseMessage response;
        try
        {
            response = await base.SendAsync(request, cancellationToken);
        }
        catch (HttpRequestException ex) when (ex.StatusCode is null && !cancellationToken.IsCancellationRequested)
        {
            var stored = await cache.GetAsync(key);
            if (stored is null)
            {
                throw;
            }

            return Replay(stored, request);
        }

        if (response.IsSuccessStatusCode && response.Content is not null)
        {
            // Buffered so the caller can still read the body after it is stored.
            await response.Content.LoadIntoBufferAsync(cancellationToken);
            var body = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.ToString();

            await cache.SetAsync(key, new CachedApiResponse((int)response.StatusCode, contentType, body), Expiry, generation);
        }
        else if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.Gone)
        {
            // Deleted on the server (e.g. a post): don't bring it back offline.
            await cache.RemoveAsync(key);
        }

        return response;
    }

    /// <summary>The cache key for a GET: the full URI, query string included (it carries paging and filters).</summary>
    public static string KeyFor(Uri requestUri) => $"api-get:{requestUri.AbsoluteUri}";

    private static HttpResponseMessage Replay(CachedApiResponse stored, HttpRequestMessage request)
    {
        var content = new ByteArrayContent(stored.Body);
        if (stored.ContentType is not null && MediaTypeHeaderValue.TryParse(stored.ContentType, out var contentType))
        {
            content.Headers.ContentType = contentType;
        }

        var response = new HttpResponseMessage((HttpStatusCode)stored.StatusCode)
        {
            Content = content,
            RequestMessage = request,
        };
        response.Headers.TryAddWithoutValidation(ReplayedHeader, "true");
        return response;
    }
}
