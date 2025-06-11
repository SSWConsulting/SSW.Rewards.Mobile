using System.Text.Json.Serialization;

namespace SSW.Rewards.Shared.DTOs.Pagination;

public interface IPaginationResult<TItem>
{
    IEnumerable<TItem> Items { get; set; }

    int Page { get; set; }
    int PageSize { get; set; }
    int Count { get; set; }

    [JsonIgnore]
    public bool IsFirstPage { get; }

    [JsonIgnore]
    public bool IsLastPage { get; }
}

public abstract class PaginationResult<TItem> : IPaginationResult<TItem>
{
    public IEnumerable<TItem> Items { get; set; } = [];

    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }

    [JsonIgnore]
    public bool IsFirstPage => Page == 0;

    [JsonIgnore]
    public bool IsLastPage => Count == 0 || (Page + 1) * PageSize >= Count;
}
