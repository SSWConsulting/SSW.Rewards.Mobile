#nullable enable

using System.Reactive.Linq;
using Akavache;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// <see cref="IFileCacheService"/> backed by Akavache's <c>LocalMachine</c> blob cache
/// (the store for cacheable, re-downloadable data). Reads never throw; writes are best-effort.
/// </summary>
public class AkavacheCacheService : IFileCacheService
{
    private readonly ILogger<AkavacheCacheService> _logger;

    public AkavacheCacheService(ILogger<AkavacheCacheService> logger)
    {
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string cacheKey)
    {
        try
        {
            return await CacheDatabase.LocalMachine.GetObject<T>(cacheKey).FirstAsync();
        }
        catch (KeyNotFoundException)
        {
            // Expected cache miss (absent or expired) — return default.
            return default;
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to read cached data for key {CacheKey}", cacheKey);
            return default;
        }
    }

    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiry = null)
    {
        try
        {
            if (expiry.HasValue)
            {
                await CacheDatabase.LocalMachine.InsertObject(cacheKey, value, expiry.Value).FirstAsync();
            }
            else
            {
                await CacheDatabase.LocalMachine.InsertObject(cacheKey, value).FirstAsync();
            }
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to cache data for key {CacheKey}", cacheKey);
        }
    }

    public async Task ResetAsync()
    {
        try
        {
            await CacheDatabase.LocalMachine.InvalidateAll().FirstAsync();
            await CacheDatabase.LocalMachine.Flush().FirstAsync();
        }
        catch (Exception e)
        {
            // Logout must never fail because the cache couldn't be cleared.
            _logger.LogError(e, "Failed to reset the cache");
        }
    }

    /// <summary>
    /// Reads any cached value first (invoking <paramref name="dataCallback"/> only when found), then
    /// fetches fresh data, caches it and invokes the callback again. Fetch failures are logged and rethrown.
    /// </summary>
    /// <typeparam name="T">Serialized type</typeparam>
    /// <param name="cacheKey">Cache key</param>
    /// <param name="fetchCallback">Callback for fetching fresh data which is also cached</param>
    /// <param name="dataCallback">Invoked with cached data (isCached=true) then fresh data (isCached=false)</param>
    /// <param name="tag">Context token forwarded to the callback so stale callbacks can be ignored</param>
    public async Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null)
    {
        var cached = await GetAsync<T>(cacheKey);
        if (cached != null)
        {
            await dataCallback.Invoke(cached, true, tag);
        }

        try
        {
            T fresh = await fetchCallback();
            await SetAsync(cacheKey, fresh);
            await dataCallback.Invoke(fresh, false, tag);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch and cache data for key {CacheKey}", cacheKey);
            throw;
        }
    }
}
