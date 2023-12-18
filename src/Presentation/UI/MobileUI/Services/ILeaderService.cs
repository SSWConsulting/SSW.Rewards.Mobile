namespace SSW.Rewards.Mobile.Services;

public interface ILeaderService
{
    Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh);
}
