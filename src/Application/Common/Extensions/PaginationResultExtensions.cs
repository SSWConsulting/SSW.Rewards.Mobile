using SSW.Rewards.Shared.DTOs.Pagination;

namespace SSW.Rewards.Application.Common.Extensions;

/// <summary>
/// This has sync and async implementation depending if it's in-memory or EF Core query.
/// </summary>
public static class PaginationResultExtensions
{
    public static TPagination ToPaginatedResult<TPagination, TItem>(this IEnumerable<TItem> query, int page, int pageSize)
        where TPagination : IPaginationResult<TItem>, new()
    {
        var count = query.Count();
        var items = query.AsQueryable().ApplyPagination(page, pageSize);

        return new()
        {
            Items = items,
            Count = count,
            Page = page,
            PageSize = pageSize
        };
    }

    public static TPagination ToPaginatedResult<TPagination, TItem>(this IEnumerable<TItem> query, IPagedRequest pagedRequest)
        where TPagination : IPaginationResult<TItem>, new()
        => query.ToPaginatedResult<TPagination, TItem>(pagedRequest.Page, pagedRequest.PageSize);

    public static async Task<TPagination> ToPaginatedResultAsync<TPagination, TItem>(this IQueryable<TItem> query, int page, int pageSize, CancellationToken ct)
        where TPagination : IPaginationResult<TItem>, new()
    {
        var count = await query
            .TagWithContext("Count")
            .CountAsync(ct);

        var items = await query
            .TagWithContext("Paged")
            .ApplyPagination(page, pageSize)
            .ToListAsync(ct);

        return new()
        {
            Items = items,
            Count = count,
            Page = page,
            PageSize = pageSize
        };
    }

    public static async Task<TPagination> ToPaginatedResultAsync<TPagination, TItem>(this IQueryable<TItem> query, IPagedRequest pagedRequest, CancellationToken ct)
        where TPagination : IPaginationResult<TItem>, new()
        => await query.ToPaginatedResultAsync<TPagination, TItem>(pagedRequest.Page, pagedRequest.PageSize, ct);
}
