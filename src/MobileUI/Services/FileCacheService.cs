using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.Services;

public interface IFileCacheService
{
    Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object, Task> dataCallback, object tag = null);
}

public class FileCacheService : IFileCacheService
{
    private static readonly string CacheDirectory = FileSystem.CacheDirectory;
    private readonly ILogger<FileCacheService> _logger;

    public FileCacheService(ILogger<FileCacheService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// This is a generic method to fetch and refresh a cache file.
    /// </summary>
    /// <typeparam name="T">Serialized type</typeparam>
    /// <param name="cacheKey">Cache key used in file name</param>
    /// <param name="fetchCallback">Callback for fetching data which are also going to be serialized into file</param>
    /// <param name="dataCallback">Callback for when we read data from storage as well as when we get fresh data from web.</param>
    /// <param name="tag">Used to check the context of the callback. If multiple callbacks are possible, this can help to determine which callbacks to ignore (eg. not latest)</param>
    public async Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object, Task> dataCallback, object tag = null)
    {
        if (!Directory.Exists(CacheDirectory))
        {
            try
            {
                Directory.CreateDirectory(CacheDirectory);
            }
            catch (Exception e)
            {
                // Log the error and keep going.
                _logger.LogError(e, "Failed to create cache directory at {CacheDirectory}", CacheDirectory);
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
                    await dataCallback?.Invoke(cached, true, tag);
                }
            }
            catch (Exception e)
            {
                // Log the error and keep going.
                _logger.LogError(e, "Failed to deserialize cached data from {FilePath}", filePath);
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
            _logger.LogError(e, "Failed to fetch and cache data for key {CacheKey}", cacheKey);
            throw;
        }
    }
}