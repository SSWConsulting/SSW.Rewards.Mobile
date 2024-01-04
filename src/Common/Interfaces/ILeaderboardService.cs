using Shared.DTOs.Leaderboard;
using Shared.Models;

namespace Shared.Interfaces;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken);
    Task<LeaderboardViewModel> GetLeaderboard(LeaderboardFilter filter, CancellationToken cancellationToken);
}
