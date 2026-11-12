using SSW.Rewards.ApiClient.Resilience;

namespace SSW.Rewards.Mobile.Helpers;

/// <summary>
/// Stores the API client's GET responses in the app cache, so the offline replay is wiped with
/// everything else on sign-out (<see cref="IFileCacheService.ResetAsync"/>).
/// </summary>
public sealed class FileCacheApiResponseCache(IFileCacheService cache) : IApiResponseCache
{
    public long Generation => cache.Generation;

    public Task<CachedApiResponse?> GetAsync(string key) => cache.GetAsync<CachedApiResponse>(key);

    public Task SetAsync(string key, CachedApiResponse response, TimeSpan expiry, long generation)
        => cache.SetAsync(key, response, expiry, generation);

    public Task RemoveAsync(string key) => cache.RemoveAsync<CachedApiResponse>(key);
}
