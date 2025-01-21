using AsyncKeyedLock;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Options;

namespace SSW.Rewards.Infrastructure.Services;

/// <summary>
/// Caching with a lock per cache key to replicate HybridCache that will have a stable release for .NET 10.
/// Once on .NET 10, we can replace it with HybridCache as hopefully it will be the final evolution of caching that we need for this project.
/// </summary>
internal sealed class CacheService : ICacheService
{
    private readonly IMemoryCache _memoryCache;
    private readonly CacheOptions _cacheSettings;
    private readonly AsyncKeyedLocker<string> _keyedLocker = new();

    public CacheService(IMemoryCache memoryCache, IOptions<CacheOptions> cacheSettings)
    {
        _memoryCache = memoryCache;
        _cacheSettings = cacheSettings.Value;
    }

    public async Task<TItem> GetOrAddAsync<TItem>(string cacheKey, Func<Task<TItem>> factory)
    {
        // In case we decide to disable it for performance or run-time issues.
        if (!_cacheSettings.Enabled)
        {
            return await factory();
        }

        if (_memoryCache.TryGetValue(cacheKey, out TItem? cacheEntry))
        {
            return cacheEntry!;
        }

        using (await _keyedLocker.LockAsync(cacheKey))
        {
            // Double-check to ensure the data was not fetched by another thread (Singleton pattern).
            if (!_memoryCache.TryGetValue(cacheKey, out cacheEntry))
            {
                cacheEntry = await factory();

                var expiration = _cacheSettings.OverrideExpiration.TryGetValue(cacheKey, out var overrideExpiration)
                    ? overrideExpiration
                    : _cacheSettings.DefaultExpiration;

                var cacheEntryOptions = new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = expiration
                };

                _memoryCache.Set(cacheKey, cacheEntry, cacheEntryOptions);
            }
        }

        return cacheEntry!;
    }

    public void Remove(params string[] cacheKeys)
    {
        foreach (var cacheKey in cacheKeys)
        {
            Remove(cacheKey);
        }
    }

    public void Remove(string cacheKey) => _memoryCache.Remove(cacheKey);
}
