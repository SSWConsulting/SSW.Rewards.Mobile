using Plugin.Firebase.Crashlytics;

namespace SSW.Rewards.Mobile.Common;

/// <summary>
/// A wrapper around ObservableCollection with purpose to have an ObservableCollection that is backed by
/// file cache, can be refreshed with live data and have performance optimizations for UI.
/// Performance is achieved by not updating the collection when not needed.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AdvancedObservableCollection<T> : IDisposable
{
    private IFileCacheService _fileCacheService;
    private CancellationTokenSource _cts;
    private string _cacheKey;
    private List<T> _fullList = [];
    private Func<bool> _shouldUseCache;
    private bool _disposed;

    public ObservableRangeCollection<T> Collection { get; } = [];
    public bool IsLoaded { get; private set; }
    public bool HasLoadedFromCache { get; private set; }
    public Func<T, T, bool> CompareItems { get; set; }
    public Func<T, bool> FilterItem { get; set; }

    public event Action<List<T>, bool> OnCollectionUpdated;
    public event Action<List<T>, bool> OnDataReceived;
    public event Func<Exception, bool> OnError;

    public void InitializeInitialCaching(IFileCacheService fileCacheService, string cacheKey, Func<bool> shouldUseCache)
    {
        _fileCacheService = fileCacheService;
        _shouldUseCache = shouldUseCache;
        _cacheKey = cacheKey;
    }

    public void RefreshCollectionWithOfflineFilter()
    {
        LoadDataAndNotify(_fullList, true, true, default);
    }

    public async Task LoadAsync(Func<CancellationToken, Task<List<T>>> fetchCallback, bool reload = false)
    {
        ThrowIfDisposed();

        try
        {
            CancelPreviousFetch();

            using (_cts = new())
            {
                CancellationToken ct = _cts.Token;
                if (_fileCacheService != null && _shouldUseCache != null && _shouldUseCache())
                {
                    HasLoadedFromCache = true;
                    await _fileCacheService.FetchAndRefresh(
                        _cacheKey,
                        async () => await fetchCallback(ct),
                        (result, isFromCache, _) =>
                        {
                            LoadDataAndNotify(result, isFromCache, reload, ct);
                            return Task.CompletedTask;
                        });
                }
                else
                {
                    var result = await fetchCallback(ct);
                    LoadDataAndNotify(result, false, reload, ct);
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
                cts.Dispose();

                _cts = null;
            }
            catch (Exception ex) when (ex is OperationCanceledException || ex is ObjectDisposedException)
            {
                // This is expected if the cancellation was already requested.
                // We can ignore this exception as it means the operation was canceled successfully.
            }
        }
    }

    private void LoadDataAndNotify(List<T> result, bool isFromCache, bool reload, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        result ??= [];
        List<T> filteredResult = ApplyFilter(result, isFromCache);
        if (reload)
        {
            _fullList = result;

            // Performance: It's cheaper to compare all items if anything changed than it is to redraw UI.
            bool shouldUpdate = ShouldReloadCollection(filteredResult);
            if (shouldUpdate)
            {
                Collection.ReplaceRange(filteredResult);
            }
        }
        else
        {
            _fullList.AddRange(result);
            Collection.AddRange(filteredResult);
        }

        IsLoaded = true;
        OnCollectionUpdated?.Invoke(filteredResult, isFromCache);
    }

    private bool ShouldReloadCollection(List<T> filteredResult)
    {
        if (Collection.Count != filteredResult.Count || CompareItems == null)
        {
            return true;
        }

        // When loading data from cache, we highly likely have same data as from web.
        // This prevents refreshing page if nothing changed which can cause flicker and
        // some cases even performance issue due to redraws if UI elements are expensive.
        for (int i = 0; i < Collection.Count; i++)
        {
            T item = Collection[i];
            if (!CompareItems(item, filteredResult[i]))
            {
                return true;
            }
        }

        return false;
    }

    private List<T> ApplyFilter(List<T> result, bool isFromCache)
    {
        OnDataReceived?.Invoke(result, isFromCache);
        if (FilterItem != null)
        {
            result = result.Where(FilterItem).ToList();
        }

        return result;
    }

    public void Reset()
    {
        CancelPreviousFetch();
        Collection.Clear();
        _fullList.Clear();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                CancelPreviousFetch();
                OnCollectionUpdated = null;
                OnDataReceived = null;
                OnError = null;
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        ObjectDisposedException.ThrowIf(_disposed, nameof(AdvancedObservableCollection<T>));
    }
}
