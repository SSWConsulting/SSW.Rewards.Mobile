using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;

public class GetLeaderboardListQuery : IRequest<LeaderboardViewModel> { }

internal class Handler : IRequestHandler<GetLeaderboardListQuery, LeaderboardViewModel>
{
    private readonly ILeaderboardService _leaderboardService;

    public Handler(ILeaderboardService leaderboardService)
    {
        _leaderboardService = leaderboardService;
    }

    public async Task<LeaderboardViewModel> Handle(GetLeaderboardListQuery request, CancellationToken cancellationToken)
    {
        var users = await _leaderboardService.GetFullLeaderboard(cancellationToken);

        return new LeaderboardViewModel { Users = users };
    }
}
