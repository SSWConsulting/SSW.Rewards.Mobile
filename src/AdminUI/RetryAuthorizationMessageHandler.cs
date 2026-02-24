using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;

namespace SSW.Rewards.Admin.UI;

/// <summary>
/// Custom authorization handler that retries failed requests on 401 responses.
/// Unlike the built-in AuthorizationMessageHandler, this will force a token refresh
/// and retry the request once before giving up.
/// </summary>
public class RetryAuthorizationMessageHandler : DelegatingHandler, IDisposable
{
    private readonly IAccessTokenProvider _tokenProvider;
    private readonly NavigationManager _navigation;
    private readonly string[] _authorizedUrls;
    private readonly string[] _scopes;

    public RetryAuthorizationMessageHandler(
        IAccessTokenProvider tokenProvider,
        NavigationManager navigation,
        IConfiguration config)
    {
        _tokenProvider = tokenProvider;
        _navigation = navigation;
        _authorizedUrls = [config.GetValue<string>("RewardsApiUrl")!];
        _scopes = ["email", "profile", "ssw-rewards-api"];
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Public kiosk leaderboard endpoint must remain reachable without OIDC login.
        if (request.RequestUri?.AbsolutePath.Contains("/api/Leaderboard/GetMobilePaginated", StringComparison.OrdinalIgnoreCase) == true)
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Check if this request should be authorized
        if (!IsAuthorizedUrl(request.RequestUri))
        {
            return await base.SendAsync(request, cancellationToken);
        }

        // Get access token and attach to request
        var tokenResult = await _tokenProvider.RequestAccessToken(
            new AccessTokenRequestOptions { Scopes = _scopes });

        if (!tokenResult.TryGetToken(out var token))
        {
            throw new AccessTokenNotAvailableException(_navigation, tokenResult, _scopes);
        }

        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token.Value);

        // Send the request
        var response = await base.SendAsync(request, cancellationToken);

        // If 401, try to refresh and retry once
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            // Force a new token by requesting with ReturnUrl
            var refreshResult = await _tokenProvider.RequestAccessToken(
                new AccessTokenRequestOptions
                {
                    Scopes = _scopes,
                    ReturnUrl = _navigation.Uri
                });

            if (refreshResult.TryGetToken(out var newToken) &&
                newToken.Value != token.Value) // Ensure we got a new token
            {
                // Clone the request for retry
                var retryRequest = await CloneRequestAsync(request);
                retryRequest.Headers.Authorization =
                    new AuthenticationHeaderValue("Bearer", newToken.Value);

                return await base.SendAsync(retryRequest, cancellationToken);
            }

            // Refresh failed - throw to trigger redirect to login
            throw new AccessTokenNotAvailableException(_navigation, refreshResult, _scopes);
        }

        return response;
    }

    private bool IsAuthorizedUrl(Uri? uri)
    {
        if (uri == null) return false;

        return _authorizedUrls.Any(url =>
            uri.AbsoluteUri.StartsWith(url, StringComparison.OrdinalIgnoreCase));
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage request)
    {
        var clone = new HttpRequestMessage(request.Method, request.RequestUri);

        if (request.Content != null)
        {
            var content = await request.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);

            foreach (var header in request.Content.Headers)
            {
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }
        }

        foreach (var header in request.Headers)
        {
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
