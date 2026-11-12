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

    // Serialises writes against resets, so a write that passed the generation check can't land after a reset.
    private readonly SemaphoreSlim _writeGate = new(1, 1);
    private long _generation;

    public AkavacheCacheService(ILogger<AkavacheCacheService> logger)
    {
        _logger = logger;
    }

    public long Generation => Interlocked.Read(ref _generation);

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

    public async Task SetAsync<T>(string cacheKey, T value, TimeSpan? expiry = null, long? generation = null)
    {
        await _writeGate.WaitAsync();
        try
        {
            if (generation is { } expected && expected != Generation)
            {
                _logger.LogInformation("Skipped caching {CacheKey}: the cache was reset while it loaded", cacheKey);
                return;
            }

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
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task RemoveAsync<T>(string cacheKey)
    {
        await _writeGate.WaitAsync();
        try
        {
            await CacheDatabase.LocalMachine.InvalidateObject<T>(cacheKey).FirstAsync();
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to remove cached data for key {CacheKey}", cacheKey);
        }
        finally
        {
            _writeGate.Release();
        }
    }

    public async Task ResetAsync()
    {
        await _writeGate.WaitAsync();
        try
        {
            // First, so loads in flight are void even if the wipe below fails.
            Interlocked.Increment(ref _generation);
            await CacheDatabase.LocalMachine.InvalidateAll().FirstAsync();
            await CacheDatabase.LocalMachine.Flush().FirstAsync();
        }
        catch (Exception e)
        {
            // Logout must never fail because the cache couldn't be cleared.
            _logger.LogError(e, "Failed to reset the cache");
        }
        finally
        {
            _writeGate.Release();
        }
    }

    /// <summary>
    /// Reads any cached value first (invoking <paramref name="dataCallback"/> only when found), then
    /// fetches fresh data, caches it and invokes the callback again. Fetch failures are logged and rethrown.
    /// A fetch that outlived a <see cref="ResetAsync"/> (logout) is dropped: neither cached nor delivered.
    /// </summary>
    /// <typeparam name="T">Serialized type</typeparam>
    /// <param name="cacheKey">Cache key</param>
    /// <param name="fetchCallback">Callback for fetching fresh data which is also cached</param>
    /// <param name="dataCallback">Invoked with cached data (isCached=true) then fresh data (isCached=false)</param>
    /// <param name="tag">Context token forwarded to the callback so stale callbacks can be ignored</param>
    public async Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null)
    {
        var generation = Generation;

        var cached = await GetAsync<T>(cacheKey);
        if (cached != null && generation == Generation)
        {
            await dataCallback.Invoke(cached, true, tag);
        }

        try
        {
            T fresh = await fetchCallback();
            if (generation != Generation)
            {
                _logger.LogInformation("Dropped fresh data for {CacheKey}: the cache was reset while it loaded", cacheKey);
                return;
            }

            await SetAsync(cacheKey, fresh, generation: generation);
            await dataCallback.Invoke(fresh, false, tag);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch and cache data for key {CacheKey}", cacheKey);
            throw;
        }
    }
}
