using SSW.Rewards.Shared.DTOs.Leaderboard;


namespace SSW.Rewards.Shared.Services;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken);
    Task<LeaderboardViewModel> GetLeaderboard(LeaderboardFilter filter, CancellationToken cancellationToken);
}
