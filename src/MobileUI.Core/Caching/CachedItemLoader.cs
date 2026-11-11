#nullable enable

using SSW.Rewards.Mobile.Common;

namespace SSW.Rewards.Mobile.Services;

/// <summary>
/// Cache-then-network load for a single item (a detail page), the counterpart of
/// <see cref="PagedListSource{T}"/> for lists: shows the cached copy at once, then the fresh one,
/// and caches the fresh copy unless the cache was reset (logout) while the fetch ran.
/// </summary>
public static class CachedItemLoader
{
    /// <param name="cache">Cache that holds the item.</param>
    /// <param name="cacheKey">Key unique to this item, e.g. <c>PostDetail_42</c>.</param>
    /// <param name="fetch">Fetches the fresh item from the network.</param>
    /// <param name="show">Displays an item; the flag is true for the cached copy.</param>
    /// <param name="ct">Cancels a superseded load: nothing more is shown and no error is reported.</param>
    /// <param name="expiry">
    /// How long the cached copy is kept. Pass one for per-item keys so items the user never reopens
    /// don't pile up until logout.
    /// </param>
    /// <returns>
    /// A failed result when the fetch fails; <see cref="ListLoadResult.HasContent"/> tells whether
    /// the cached copy is on screen, so the caller can stay quiet behind it.
    /// </returns>
    public static async Task<ListLoadResult> LoadAsync<T>(
        IFileCacheService cache,
        string cacheKey,
        Func<CancellationToken, Task<T>> fetch,
        Func<T, bool, Task> show,
        CancellationToken ct = default,
        TimeSpan? expiry = null)
        where T : class
    {
        // Captured before any await: a cache reset (logout) during this load voids its write.
        var generation = cache.Generation;
        var shownCached = false;

        try
        {
            var cached = await cache.GetAsync<T>(cacheKey);
            if (cached is not null && !ct.IsCancellationRequested)
            {
                await show(cached, true);
                shownCached = true;
            }

            var fresh = await fetch(ct);
            if (ct.IsCancellationRequested)
            {
                return ListLoadResult.Ok(shownCached, shownCached);
            }

            await show(fresh, false);
            await cache.SetAsync(cacheKey, fresh, expiry, generation);
            return ListLoadResult.Ok(hasContent: true, fromCache: false);
        }
        catch (Exception) when (ct.IsCancellationRequested)
        {
            return ListLoadResult.Ok(shownCached, shownCached);
        }
        catch (Exception ex)
        {
            return ListLoadResult.Fail(ex, shownCached);
        }
    }
}
