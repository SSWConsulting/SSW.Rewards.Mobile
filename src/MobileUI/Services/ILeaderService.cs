using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Mobile.Services;

public interface ILeaderService
{
    Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh);
}
