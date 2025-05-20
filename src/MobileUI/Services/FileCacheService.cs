using System.Text.Json;

namespace SSW.Rewards.Mobile.Services;

public interface IFileCacheService
{
    Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Action<T, bool>? callback = null);
    Task<(T? value, bool fromCache)> FetchAndRefreshValue<T>(string cacheKey, Func<Task<T>> fetchCallback);
}

public class FileCacheService : IFileCacheService
{
    private static readonly string CacheDirectory = FileSystem.CacheDirectory;

    public async Task<(T? value, bool fromCache)> FetchAndRefreshValue<T>(string cacheKey, Func<Task<T>> fetchCallback)
    {
        var filePath = Path.Combine(CacheDirectory, cacheKey + ".json");
        if (File.Exists(filePath))
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                var cached = await JsonSerializer.DeserializeAsync<T>(stream);
                if (cached != null)
                {
                    return (cached, true);
                }
            }
            catch { /* Ignore cache read errors */ }
        }

        try
        {
            var fresh = await fetchCallback();
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, fresh);
            return (fresh, false);
        }
        catch
        {
            return (default, false);
        }
    }

    public async Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Action<T, bool>? callback = null)
    {
        var filePath = Path.Combine(CacheDirectory, cacheKey + ".json");
        bool cacheHit = false;
        if (File.Exists(filePath))
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                var cached = await JsonSerializer.DeserializeAsync<T>(stream);
                if (cached != null)
                {
                    cacheHit = true;
                    callback?.Invoke(cached, true);
                }
            }
            catch { /* Ignore cache read errors */ }
        }

        if (cacheHit && callback == null)
            return;

        try
        {
            var fresh = await fetchCallback();
            callback?.Invoke(fresh, false);
            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, fresh);
        }
        catch { /* Ignore fetch errors, user still sees cached data */ }
    }
}
