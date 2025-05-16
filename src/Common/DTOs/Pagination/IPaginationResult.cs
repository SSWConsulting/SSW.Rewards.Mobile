namespace SSW.Rewards.Shared.DTOs.Pagination;

public interface IPaginationResult<TItem>
{
    IEnumerable<TItem> Items { get; set; }

    int Page { get; set; }
    int PageSize { get; set; }
    int Count { get; set; }
}
