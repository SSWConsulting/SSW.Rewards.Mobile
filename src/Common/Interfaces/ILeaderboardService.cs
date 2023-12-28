using Shared.DTOs.Leaderboard;
using Shared.Models;

namespace Shared.Interfaces;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboard();
    Task<LeaderboardViewModel> GetLeaderboard(LeaderboardFilter filter);
}
