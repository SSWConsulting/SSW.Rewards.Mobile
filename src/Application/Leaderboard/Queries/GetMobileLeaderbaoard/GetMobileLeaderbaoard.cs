using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetMobileLeaderbaoard;

public class GetMobileLeaderboardQuery : IRequest<MobileLeaderboardViewModel>, IPagedRequest
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public LeaderboardFilter CurrentPeriod { get; set; }
}

internal class GetMobileLeaderboardQueryHandler(ILeaderboardService leaderboardService, ICurrentUserService currentUserService) : IRequestHandler<GetMobileLeaderboardQuery, MobileLeaderboardViewModel>
{
    public async Task<MobileLeaderboardViewModel> Handle(GetMobileLeaderboardQuery request, CancellationToken cancellationToken)
    {
        string currentUserEmail = currentUserService.GetUserEmail();

        List<LeaderboardUserDto> users = await leaderboardService.GetFullLeaderboard(cancellationToken);

        // Remap to a lighter DTO for the Kiosk leaderboard with only what we need.
        IEnumerable<MobileLeaderboardUserDto> query = request.CurrentPeriod switch
        {
            LeaderboardFilter.ThisMonth => users.Select(x => Map(x, x.PointsThisMonth, currentUserEmail)),
            LeaderboardFilter.ThisYear => users.Select(x => Map(x, x.PointsThisYear, currentUserEmail)),
            LeaderboardFilter.ThisWeek => users.Select(x => Map(x, x.PointsThisWeek, currentUserEmail)),
            _ => users.OrderByDescending(x => x.TotalPoints).Select(x => Map(x, x.TotalPoints, currentUserEmail))
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

        // Return current rank of the authenticated user for easier logic on client.
        if (!string.IsNullOrWhiteSpace(currentUserEmail))
        {
            // First try to search through serialized page as that would be fastest, than rescan previous query.
            var currentUser = result.Items.FirstOrDefault(x => x.IsMe)
                ?? query.FirstOrDefault(x => x.IsMe);

            result.MyRank = currentUser?.Rank ?? 0;
        }

        return result;
    }

    private static MobileLeaderboardUserDto Map(LeaderboardUserDto user, int selectedPoints, string currentUserEmail)
        => new()
        {
            Name = user.Name,
            Rank = user.Rank,
            Title = user.Title,
            UserId = user.UserId,
            Points = selectedPoints,
            ProfilePic = user.ProfilePic,

            IsMe = string.Equals(user.Email, currentUserEmail, StringComparison.OrdinalIgnoreCase)
        };
}
