using System.Text.Json;
using Plugin.Firebase.Crashlytics;

namespace SSW.Rewards.Mobile.Services;

public interface IFileCacheService
{
    Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Action<T, bool, object> callback = null, object tag = null);
}

public class FileCacheService : IFileCacheService
{
    private static readonly string CacheDirectory = FileSystem.CacheDirectory;

    /// <summary>
    /// This is a generic method to fetch and refresh a cache file.
    /// </summary>
    /// <typeparam name="T">Serialized type</typeparam>
    /// <param name="cacheKey">Cache key used in file name</param>
    /// <param name="fetchCallback">Callback for fetching data which are also going to be serialized into file</param>
    /// <param name="dataCallback">Callback for when we read data from storage as well as when we get fresh data from web.</param>
    /// <param name="tag">Used to check the context of the callback. If multiple callbacks are possible, this can help to determine which callbacks to ignore (eg. not latest)</param>
    public async Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Action<T, bool, object> dataCallback, object tag = null)
    {
        if (!Directory.Exists(CacheDirectory))
        {
            try
            {
                Directory.CreateDirectory(CacheDirectory);
            }
            catch (Exception e)
            {
                // Log the error to Firebase Crashlytics and keep going.
                CrossFirebaseCrashlytics.Current.RecordException(e);
            }
        }

        var filePath = Path.Combine(CacheDirectory, cacheKey + ".json");
        if (File.Exists(filePath))
        {
            try
            {
                using var stream = File.OpenRead(filePath);
                var cached = await JsonSerializer.DeserializeAsync<T>(stream);
                if (cached != null)
                {
                    dataCallback?.Invoke(cached, true, tag);
                }
            }
            catch (Exception e)
            {
                // Log the error to Firebase Crashlytics and keep going.
                CrossFirebaseCrashlytics.Current.RecordException(e);
            }
        }

        try
        {
            T fresh = await fetchCallback();

            using var stream = File.Create(filePath);
            await JsonSerializer.SerializeAsync(stream, fresh);

            dataCallback?.Invoke(fresh, false, tag);
        }
        catch (Exception e)
        {
            CrossFirebaseCrashlytics.Current.RecordException(e);
            throw;
        }
    }
}
