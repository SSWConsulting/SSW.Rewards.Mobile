namespace SSW.Rewards.Application.Common.Interfaces;

public interface IPagedRequest
{
    public int Page { get; }
    public int PageSize { get; }
}
