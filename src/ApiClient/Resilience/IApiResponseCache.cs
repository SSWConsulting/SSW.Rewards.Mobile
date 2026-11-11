namespace SSW.Rewards.ApiClient.Resilience;

/// <summary>
/// Stores successful GET responses so <see cref="OfflineFallbackHandler"/> can replay them when the
/// network fails. Register one to make every GET work offline; without a registration the client
/// never caches. It holds user data, so it must be wiped on sign-out.
/// </summary>
public interface IApiResponseCache
{
    /// <summary>
    /// Changes whenever the cache is wiped. Captured before a request and passed back to
    /// <see cref="SetAsync"/>, so a response that outlived a wipe (sign-out) isn't stored.
    /// </summary>
    long Generation { get; }

    /// <summary>The stored response, or null when absent or unreadable. Never throws.</summary>
    Task<CachedApiResponse?> GetAsync(string key);

    /// <summary>Best-effort write, skipped when the cache was wiped since <paramref name="generation"/>. Never throws.</summary>
    Task SetAsync(string key, CachedApiResponse response, TimeSpan expiry, long generation);
}

/// <summary>A stored response body and the headers needed to read it back.</summary>
public sealed record CachedApiResponse(int StatusCode, string? ContentType, byte[] Body);
