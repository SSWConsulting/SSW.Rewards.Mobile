namespace SSW.Rewards.Mobile.Common;

public class AdvancedObservableCollection<T>
{
    private IFileCacheService _fileCacheService;
    private CancellationTokenSource _cts;
    private string _cacheKey;
    private bool _loaded;

    public ObservableRangeCollection<T> Collection { get; } = [];
    public bool IsLoaded => _loaded;

    public event Action<List<T>, bool> OnDataReceived;
    public event Func<T, T, bool> OnCompareItems;
    public event Func<T, bool> OnFilterItem;
    public event Func<bool> OnUseCache;

    public void InitializeInitialCaching(IFileCacheService fileCacheService, string cacheKey)
    {
        _fileCacheService = fileCacheService;
        _cacheKey = cacheKey;
    }

    public async Task LoadAsync(Func<CancellationToken, Task<List<T>>> fetchCallback, bool reload = false)
    {
        try
        {
            await _cts?.CancelAsync();

            using (_cts = new())
            {
                CancellationToken ct = _cts.Token;
                if (_fileCacheService != null && OnUseCache != null && OnUseCache())
                {
                    await _fileCacheService.FetchAndRefresh(
                        _cacheKey,
                        async () => await fetchCallback(ct),
                        (result, isFromCache, _) =>
                        {
                            UpdateCollectionAndNotify(result, isFromCache, reload, ct);
                            return Task.CompletedTask;
                        });
                }
                else
                {
                    var result = await fetchCallback(ct);
                    UpdateCollectionAndNotify(result, false, reload, ct);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Handle cancellation gracefully, if needed
        }
        finally
        {
            await _cts?.CancelAsync();
        }
        
    }

    private void UpdateCollectionAndNotify(List<T> result, bool isFromCache, bool reload, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        if (OnFilterItem != null)
        {
            result = result.Where(OnFilterItem).ToList();
        }

        if (reload)
        {
            bool shouldUpdate = true;
            if (Collection.Count == result.Count && !reload && OnCompareItems != null)
            {
                // When loading data from cache, we highly likely have same data as from web.
                // This prevents refreshing page if nothing changed.
                shouldUpdate = false;
                for (int i = 0; i < Collection.Count; i++)
                {
                    T item = Collection[i];
                    if (!OnCompareItems(item, result[i]))
                    {
                        shouldUpdate = true;
                        break;
                    }
                }
            }

            if (shouldUpdate)
            {
                Collection.ReplaceRange(result);
            }
        }
        else
        {
            Collection.AddRange(result);
        }

        _loaded = true;
        OnDataReceived?.Invoke(result, isFromCache);
    }

    public void CancelCurrentFetch()
    {
        _cts?.Cancel();
    }

    public void Reset()
    {
        CancelCurrentFetch();
        Collection.Clear();
    }
}
