using Plugin.Firebase.Crashlytics;

namespace SSW.Rewards.Mobile.Common;

public class AdvancedObservableCollection<T>
{
    private IFileCacheService _fileCacheService;
    private CancellationTokenSource _cts;
    private string _cacheKey;

    public ObservableRangeCollection<T> Collection { get; } = [];
    public bool IsLoaded { get; private set; }
    public bool HasLoadedFromCache { get; private set; }

    public event Action<List<T>, bool> OnCollectionUpdated;
    public event Action<List<T>, bool> OnDataReceived;
    public event Func<T, T, bool> OnCompareItems;
    public event Func<Exception, bool> OnError;
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
            CancelPreviousFetch();

            using (_cts = new())
            {
                CancellationToken ct = _cts.Token;
                if (_fileCacheService != null && OnUseCache != null && OnUseCache())
                {
                    HasLoadedFromCache = true;
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

            _cts = null;
        }
        catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
        {
            // Handle cancellation gracefully, if needed
        }
        catch (Exception ex)
        {
            CrossFirebaseCrashlytics.Current.RecordException(ex);

            // Ask ViewModel if it wants to handle the error. Otherwise, rethrow it.
            if (OnError == null || !OnError(ex))
            {
                throw;
            }
        }
        finally
        {
            CancelPreviousFetch();
        }
    }

    private void CancelPreviousFetch()
    {
        CancellationTokenSource cts = _cts;
        if (cts != null && !cts.IsCancellationRequested)
        {
            try
            {
                cts.Cancel();
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
            {
                // This is expected if the cancellation was already requested.
                // We can ignore this exception as it means the operation was canceled successfully.
            }
        }
    }

    private void UpdateCollectionAndNotify(List<T> result, bool isFromCache, bool reload, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        OnDataReceived?.Invoke(result, isFromCache);
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

        IsLoaded = true;
        OnCollectionUpdated?.Invoke(result, isFromCache);
    }

    public void Reset()
    {
        CancelPreviousFetch();
        Collection.Clear();
    }
}
