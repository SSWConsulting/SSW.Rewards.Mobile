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
        var query = OrderByLeaderboardPeriod(users, request.CurrentPeriod);

        return new LeaderboardViewModel
        {
            Users = query
                .Skip(request.Skip)
                .Take(request.Take)
                .ToArray()
        };
    }

    private static IOrderedEnumerable<LeaderboardUserDto> OrderByLeaderboardPeriod(IEnumerable<LeaderboardUserDto> users, LeaderboardFilter currentPeriod)
        => currentPeriod switch
        {
            LeaderboardFilter.ThisMonth => OrderByPeriodPoints(users, user => user.PointsThisMonth),
            LeaderboardFilter.ThisYear => OrderByPeriodPoints(users, user => user.PointsThisYear),
            LeaderboardFilter.ThisWeek => OrderByPeriodPoints(users, user => user.PointsThisWeek),
            _ => OrderByAllTimeRank(users),
        };

    private static IOrderedEnumerable<LeaderboardUserDto> OrderByPeriodPoints(IEnumerable<LeaderboardUserDto> users, Func<LeaderboardUserDto, int> periodPoints)
        => users
            .OrderByDescending(periodPoints)
            .ThenBy(user => user.Rank)
            .ThenBy(user => user.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(user => user.UserId);

    private static IOrderedEnumerable<LeaderboardUserDto> OrderByAllTimeRank(IEnumerable<LeaderboardUserDto> users)
        => users
            .OrderBy(user => user.Rank)
            .ThenBy(user => user.Name ?? string.Empty, StringComparer.OrdinalIgnoreCase)
            .ThenBy(user => user.UserId);
}
