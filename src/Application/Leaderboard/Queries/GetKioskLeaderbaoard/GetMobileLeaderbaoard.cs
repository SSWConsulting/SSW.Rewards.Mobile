using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetKioskLeaderbaoard;

public class GetMobileLeaderboardQuery : IRequest<MobileLeaderboardViewModel>, IPagedRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public LeaderboardFilter CurrentPeriod { get; set; }
}

internal class GetMobileLeaderboardQueryHandler(ILeaderboardService leaderboardService) : IRequestHandler<GetMobileLeaderboardQuery, MobileLeaderboardViewModel>
{
    public async Task<MobileLeaderboardViewModel> Handle(GetMobileLeaderboardQuery request, CancellationToken cancellationToken)
    {
        List<LeaderboardUserDto> users = await leaderboardService.GetFullLeaderboard(cancellationToken);

        // Remap to a lighter DTO for the Kiosk leaderboard with only what we need.
        IEnumerable<MobileLeaderboardUserDto> query = request.CurrentPeriod switch
        {
            LeaderboardFilter.ThisMonth => users.Select(x => Map(x, x.PointsThisMonth)),
            LeaderboardFilter.ThisYear => users.Select(x => Map(x, x.PointsThisYear)),
            LeaderboardFilter.ThisWeek => users.Select(x => Map(x, x.PointsThisWeek)),
            _ => users.OrderByDescending(x => x.TotalPoints).Select(x => Map(x, x.PointsThisMonth)),
        };

        // Sort and recalculate the rank based on the current period.
        query = query
            .OrderByDescending(x => x.Points)
            .Select((x, i) =>
            {
                x.Rank = i + 1;
                return x;
            });

        var result = query.ToPaginatedResult<MobileLeaderboardViewModel, MobileLeaderboardUserDto>(request);
        result.CurrentPeriod = request.CurrentPeriod;
        return result;
    }

    private static MobileLeaderboardUserDto Map(LeaderboardUserDto user, int selectedPoints)
        => new()
        {
            Name = user.Name,
            Rank = user.Rank,
            UserId = user.UserId,
            Points = selectedPoints,
            ProfilePic = user.ProfilePic
        };
}
