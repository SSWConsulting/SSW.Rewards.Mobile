using Shared.DTOs.Leaderboard;


namespace Shared.Interfaces;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken);
    Task<LeaderboardViewModel> GetLeaderboard(LeaderboardFilter filter, CancellationToken cancellationToken);
}
