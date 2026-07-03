namespace SSW.Rewards.Mobile.Services;

public interface IFileCacheService
{
    /// <summary>Reads a cached value, or default when absent/unreadable. Never throws.</summary>
    Task<T?> GetAsync<T>(string cacheKey);

    /// <summary>Writes a value to the cache with an optional relative expiry. Best-effort — never throws.</summary>
    Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiry = null);

    Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null);
}
