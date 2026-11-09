namespace SSW.Rewards.Mobile.Services;

public interface IFileCacheService
{
    /// <summary>
    /// Changes on every <see cref="ResetAsync"/>. A load captures it before fetching and passes it
    /// to <see cref="SetAsync{T}"/>, so a load that outlived a reset (e.g. logout) can't write the
    /// previous session's data back.
    /// </summary>
    long Generation { get; }

    /// <summary>Reads a cached value, or default when absent/unreadable. Never throws.</summary>
    Task<T?> GetAsync<T>(string cacheKey);

    /// <summary>
    /// Writes a value to the cache with an optional relative expiry. Best-effort — never throws.
    /// With <paramref name="generation"/>, the write is skipped when the cache was reset since.
    /// </summary>
    Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiry = null, long? generation = null);

    /// <summary>Wipes every cached entry (e.g. on logout, so no user data outlives the session). Never throws.</summary>
    Task ResetAsync();

    /// <summary>
    /// Invokes <paramref name="dataCallback"/> with cached data (if any), then fetches, caches and invokes it with fresh data.
    /// Fresh data from a fetch that outlived a <see cref="ResetAsync"/> is dropped: neither cached nor delivered.
    /// </summary>
    Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null);
}
