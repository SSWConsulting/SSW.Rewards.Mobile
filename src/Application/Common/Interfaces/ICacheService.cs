
namespace SSW.Rewards.Application.Common.Interfaces;

public interface ICacheService
{
    Task<TItem> GetOrAddAsync<TItem>(string cacheKey, Func<Task<TItem>> factory);
    void Remove(string cacheKey);
    void Remove(params string[] cacheKeys);
}
