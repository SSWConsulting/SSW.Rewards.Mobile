#nullable enable

using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Mobile.Services;

public interface IFileCacheService
{
    /// <summary>Reads a cached value, or default when absent/unreadable. Never throws.</summary>
    Task<T?> GetAsync<T>(string cacheKey);

    /// <summary>Writes a value to the cache. Best-effort — never throws.</summary>
    Task SetAsync<T>(string cacheKey, T value);

    Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null);
}

public class FileCacheService : IFileCacheService
{
    private static readonly string CacheDirectory = FileSystem.CacheDirectory;
    private readonly ILogger<FileCacheService> _logger;

    public FileCacheService(ILogger<FileCacheService> logger)
    {
        _logger = logger;
    }

    public async Task<T?> GetAsync<T>(string cacheKey)
    {
        var filePath = GetFilePath(cacheKey);
        if (!File.Exists(filePath))
        {
            return default;
        }

        try
        {
            using var stream = File.OpenRead(filePath);
            return await JsonSerializer.DeserializeAsync<T>(stream);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to deserialize cached data from {FilePath}", filePath);
            return default;
        }
    }

    public async Task SetAsync<T>(string cacheKey, T value)
    {
        try
        {
            EnsureCacheDirectory();
            using var stream = File.Create(GetFilePath(cacheKey));
            await JsonSerializer.SerializeAsync(stream, value);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to cache data for key {CacheKey}", cacheKey);
        }
    }

    /// <summary>
    /// This is a generic method to fetch and refresh a cache file.
    /// </summary>
    /// <typeparam name="T">Serialized type</typeparam>
    /// <param name="cacheKey">Cache key used in file name</param>
    /// <param name="fetchCallback">Callback for fetching data which are also going to be serialized into file</param>
    /// <param name="dataCallback">Callback for when we read data from storage as well as when we get fresh data from web.</param>
    /// <param name="tag">Used to check the context of the callback. If multiple callbacks are possible, this can help to determine which callbacks to ignore (eg. not latest)</param>
    public async Task FetchAndRefresh<T>(string cacheKey, Func<Task<T>> fetchCallback, Func<T, bool, object?, Task> dataCallback, object? tag = null)
    {
        var cached = await GetAsync<T>(cacheKey);
        if (cached != null)
        {
            await dataCallback.Invoke(cached, true, tag);
        }

        try
        {
            T fresh = await fetchCallback();
            await SetAsync(cacheKey, fresh);
            dataCallback.Invoke(fresh, false, tag);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Failed to fetch and cache data for key {CacheKey}", cacheKey);
            throw;
        }
    }

    private static string GetFilePath(string cacheKey) => Path.Combine(CacheDirectory, cacheKey + ".json");

    private void EnsureCacheDirectory()
    {
        if (!Directory.Exists(CacheDirectory))
        {
            Directory.CreateDirectory(CacheDirectory);
        }
    }
}
