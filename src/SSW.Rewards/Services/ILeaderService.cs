namespace SSW.Rewards.Services;

public interface ILeaderService
{
    Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh);
}
