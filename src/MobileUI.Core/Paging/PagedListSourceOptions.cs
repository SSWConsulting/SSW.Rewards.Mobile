using SSW.Rewards.Mobile.Services;

namespace SSW.Rewards.Mobile.Common;

/// <summary>Configuration for a <see cref="PagedListSource{T}"/>. Only FetchPage is required.</summary>
public sealed class PagedListSourceOptions<T>
{
    public required Func<int, int, CancellationToken, Task<List<T>>> FetchPage { get; init; }

    public int PageSize { get; init; } = 50;

    /// <summary>Display preparation applied to every item, cached or fresh (relative timestamps, fallbacks).</summary>
    public Action<T>? PrepareItem { get; init; }

    /// <summary>Anti-flicker check. When every item matches, the shown instances are kept — compare every displayed field. Omit to always redraw.</summary>
    public Func<T, T, bool>? AreSame { get; init; }

    public IFileCacheService? Cache { get; init; }
    public string? CacheKey { get; init; }

    /// <summary>Extra cache gate, e.g. only the default segment of a segmented page.</summary>
    public Func<bool>? ShouldUseCache { get; init; }
}
