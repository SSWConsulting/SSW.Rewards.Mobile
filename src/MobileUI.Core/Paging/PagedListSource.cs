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
    /// Replaces the list: shows cached items immediately (first load only),
    /// then the fresh first page. Safe to call repeatedly (refresh, segment switch).
    /// </summary>
    public async Task<ListLoadResult> RefreshAsync()
    {
        var ct = RestartToken();
        _fetchedCount = 0;
        _endReached = false;

        try
        {
            if (UseCache && Items.Count == 0)
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
                await _options.Cache!.SetAsync(_options.CacheKey!, fresh);
            }

            return ListLoadResult.Ok(Items.Count > 0, fromCache: false);
        }
        catch (OperationCanceledException)
        {
            return ListLoadResult.Ok(Items.Count > 0, IsShowingCachedData);
        }
        catch (Exception ex)
        {
            return ListLoadResult.Fail(ex, Items.Count > 0);
        }
    }

    /// <summary>
    /// Appends the next page (network-only). A failed page can be retried;
    /// only a successful short/empty page marks the end of the list.
    /// </summary>
    public async Task<ListLoadResult> LoadMoreAsync()
    {
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
        catch (OperationCanceledException)
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
