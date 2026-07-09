using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Common;

/// <summary>Configuration for a <see cref="PagedListSource{T}"/>. Only FetchPage is required.</summary>
public sealed class PagedListSourceOptions<T>
{
    public required Func<int, int, CancellationToken, Task<List<T>>> FetchPage { get; init; }

    public int PageSize { get; init; } = 50;

    /// <summary>Display preparation applied to every item, cached or fresh (relative timestamps, fallbacks).</summary>
    public Action<T>? PrepareItem { get; init; }

    /// <summary>Item equality for the anti-flicker check. Omit to always redraw on refresh.</summary>
    public Func<T, T, bool>? AreSame { get; init; }

    public IFileCacheService? Cache { get; init; }
    public string? CacheKey { get; init; }

    /// <summary>Extra cache gate, e.g. only the default segment of a segmented page.</summary>
    public Func<bool>? ShouldUseCache { get; init; }
}
