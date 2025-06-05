namespace SSW.Rewards.Mobile.Services;

public class ObservableCacheCollectionService<T>
{
    private readonly IFileCacheService _fileCacheService;
    private readonly string _cacheKey;
    private CancellationTokenSource? _cts;
    private bool _loaded;

    public ObservableRangeCollection<T> Collection { get; } = [];

    public event Action<List<T>, bool> OnDataReceived;

    public ObservableCacheCollectionService(IFileCacheService fileCacheService, string cacheKey)
    {
        _fileCacheService = fileCacheService;
        _cacheKey = cacheKey;
    }

    public async Task LoadAsync(Func<CancellationToken, Task<List<T>>> fetchCallback, bool reload = false)
    {
        _cts?.Cancel();
        _cts = new();
        CancellationToken ct = _cts.Token;

        if (!_loaded)
        {
            await _fileCacheService.FetchAndRefresh(
                _cacheKey,
                async () => await fetchCallback(ct),
                async (result, isFromCache, _) => UpdateCollectionAndNotify(result, isFromCache, reload, ct));
        }
        else
        {
            var result = await fetchCallback(ct);
            UpdateCollectionAndNotify(result, false, reload, ct);
        }
    }

    private void UpdateCollectionAndNotify(List<T> result, bool isFromCache, bool reload, CancellationToken ct)
    {
        if (ct.IsCancellationRequested)
        {
            return;
        }

        if (reload)
        {
            Collection.ReplaceRange(result);
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
