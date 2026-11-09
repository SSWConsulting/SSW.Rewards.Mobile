using CommunityToolkit.Mvvm.ComponentModel;
using SSW.Rewards.Mobile.Helpers;

namespace SSW.Rewards.Mobile.Common;

/// <summary>
/// A bindable, offline-friendly backing source for paged list pages.
/// Owns the whole load lifecycle so ViewModels don't have to: cache-then-network
/// initial load, pull-to-refresh, network-only pagination with end-of-feed
/// detection, per-item display preparation, anti-flicker replace, and
/// cancellation of stale loads. Bind <see cref="Items"/> in XAML.
/// </summary>
public sealed partial class PagedListSource<T> : ObservableObject
{
    private readonly PagedListSourceOptions<T> _options;
    private CancellationTokenSource? _cts;

    // Number of items confirmed from the network; the skip for the next page.
    // Stays 0 while only cached data is shown so pagination can't duplicate page 1.
    private int _fetchedCount;
    private bool _endReached;
    private bool _isRefreshing;
    private bool _loadMoreQueued;
    private TaskCompletionSource _refreshIdle = new();

    // False while Items still hold the previous query's list (segment or filter switch in flight).
    private bool _showsCurrentQuery = true;

    public ObservableRangeCollection<T> Items { get; } = [];

    [ObservableProperty]
    public partial bool IsShowingCachedData { get; private set; }

    public PagedListSource(PagedListSourceOptions<T> options)
    {
        if ((options.Cache is null) != (options.CacheKey is null))
        {
            throw new ArgumentException("Cache and CacheKey must be provided together.", nameof(options));
        }

        _options = options;
    }

    /// <summary>
    /// Replaces the list: shows cached items immediately (first load or query switch),
    /// then the fresh first page. Safe to call repeatedly (refresh, segment switch).
    /// A failed refresh keeps the shown items and their pagination position, unless they
    /// belong to the previous query: those are cleared rather than shown under the new one.
    /// </summary>
    /// <param name="queryChanged">
    /// True when <see cref="PagedListSourceOptions{T}.FetchPage"/> now returns a different list
    /// (segment or filter switch), so the shown items can no longer be paged.
    /// </param>
    public async Task<ListLoadResult> RefreshAsync(bool queryChanged = false)
    {
        var ct = RestartToken();
        if (!_isRefreshing)
        {
            _refreshIdle = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            _isRefreshing = true;
        }
        if (queryChanged)
        {
            _fetchedCount = 0;
            _endReached = false;
            _showsCurrentQuery = Items.Count == 0;
        }

        // Captured before any await: a cache reset (logout) during this load voids its write.
        var cacheGeneration = _options.Cache?.Generation ?? 0;

        try
        {
            if (UseCache && (Items.Count == 0 || !_showsCurrentQuery))
            {
                var cached = await _options.Cache!.GetAsync<List<T>>(_options.CacheKey!);
                if (cached is { Count: > 0 } && !ct.IsCancellationRequested)
                {
                    Show(cached, fromCache: true);
                }
            }

            var fresh = await _options.FetchPage(0, _options.PageSize, ct);
            if (ct.IsCancellationRequested)
            {
                return ListLoadResult.Ok(Items.Count > 0, IsShowingCachedData);
            }

            _fetchedCount = fresh.Count;
            _endReached = fresh.Count < _options.PageSize;
            Show(fresh, fromCache: false);

            if (UseCache)
            {
                await _options.Cache!.SetAsync(_options.CacheKey!, fresh, generation: cacheGeneration);
            }

            return ListLoadResult.Ok(Items.Count > 0, fromCache: false);
        }
        catch (Exception) when (ct.IsCancellationRequested)
        {
            // Superseded: whatever it hit (even a 401) is no longer the user's problem.
            return ListLoadResult.Ok(Items.Count > 0, IsShowingCachedData);
        }
        catch (Exception ex)
        {
            if (!_showsCurrentQuery)
            {
                // The previous query's items would be mislabelled under the new one.
                Items.Clear();
                IsShowingCachedData = false;
                _showsCurrentQuery = true;
            }

            return ListLoadResult.Fail(ex, Items.Count > 0);
        }
        finally
        {
            // A superseding refresh owns the flag now.
            if (!ct.IsCancellationRequested)
            {
                _isRefreshing = false;
                _refreshIdle.TrySetResult();
            }
        }
    }

    /// <summary>
    /// Appends the next page (network-only). A failed page can be retried;
    /// only a successful short/empty page marks the end of the list.
    /// Requested during a refresh, it runs once the refresh settles (at most one waits).
    /// </summary>
    public async Task<ListLoadResult> LoadMoreAsync()
    {
        if (_isRefreshing)
        {
            if (_loadMoreQueued)
            {
                return ListLoadResult.Ok(Items.Count > 0, IsShowingCachedData);
            }

            // The scroll threshold may not fire again if the refresh changes nothing visible.
            _loadMoreQueued = true;
            try
            {
                while (_isRefreshing)
                {
                    await _refreshIdle.Task;
                }
            }
            finally
            {
                _loadMoreQueued = false;
            }
        }

        // No successful refresh yet (e.g. showing cache offline) — appending could duplicate page 1.
        if (_endReached || _fetchedCount == 0)
        {
            return ListLoadResult.Ok(Items.Count > 0, IsShowingCachedData);
        }

        var ct = RestartToken();

        try
        {
            var page = await _options.FetchPage(_fetchedCount, _options.PageSize, ct);
            if (ct.IsCancellationRequested)
            {
                return ListLoadResult.Ok(Items.Count > 0, IsShowingCachedData);
            }

            _fetchedCount += page.Count;
            _endReached = page.Count < _options.PageSize;
            if (_options.PrepareItem is { } prepare)
            {
                page.ForEach(prepare);
            }
            Items.AddRange(page);

            return ListLoadResult.Ok(hasContent: true, fromCache: false);
        }
        catch (Exception) when (ct.IsCancellationRequested)
        {
            return ListLoadResult.Ok(Items.Count > 0, IsShowingCachedData);
        }
        catch (Exception ex)
        {
            return ListLoadResult.Fail(ex, Items.Count > 0);
        }
    }

    private bool UseCache =>
        _options.Cache is not null && (_options.ShouldUseCache?.Invoke() ?? true);

    private void Show(List<T> items, bool fromCache)
    {
        if (_options.PrepareItem is { } prepare)
        {
            items.ForEach(prepare);
        }

        // Cache and network are usually identical — skipping the replace avoids a full redraw.
        if (!SameAsShown(items))
        {
            Items.ReplaceRange(items);
        }

        IsShowingCachedData = fromCache;
        _showsCurrentQuery = true;
    }

    private bool SameAsShown(List<T> items)
    {
        if (_options.AreSame is null || Items.Count != items.Count)
        {
            return false;
        }

        for (int i = 0; i < items.Count; i++)
        {
            if (!_options.AreSame(Items[i], items[i]))
            {
                return false;
            }
        }

        return true;
    }

    private CancellationToken RestartToken()
    {
        _cts?.Cancel();
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        return _cts.Token;
    }
}
