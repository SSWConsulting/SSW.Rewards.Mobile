using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardPaginatedList;

public class GetLeaderboardPaginatedListQuery : IRequest<LeaderboardViewModel> 
{
    public int Skip { get; set; }
    public int Take { get; set; }
    public LeaderboardFilter CurrentPeriod { get; set; }
}

internal class Handler : IRequestHandler<GetLeaderboardPaginatedListQuery, LeaderboardViewModel>
{
    private readonly ILeaderboardService _leaderboardService;

    public Handler(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    public async Task<LeaderboardViewModel> Handle(GetLeaderboardPaginatedListQuery request, CancellationToken cancellationToken)
    {
        List<LeaderboardUserDto> users = await _leaderboardService.GetFullLeaderboard(cancellationToken);
        var query = users.AsQueryable();
        query = request.CurrentPeriod switch
        {
            LeaderboardFilter.ThisMonth => query.OrderByDescending(x => x.PointsThisMonth),
            LeaderboardFilter.ThisYear => query.OrderByDescending(x => x.PointsThisYear),
            LeaderboardFilter.Forever => query.OrderByDescending(x => x.TotalPoints),
            LeaderboardFilter.ThisWeek => query.OrderByDescending(x => x.PointsThisWeek),
            _ => query.OrderByDescending(x => x.TotalPoints),
        };

        return new LeaderboardViewModel
        {
            Users = query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToArray()
        };
    }
}
