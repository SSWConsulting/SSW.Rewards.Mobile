namespace SSW.Rewards.Shared.DTOs.Leaderboard;

public class LeaderboardViewModel
{
    public IEnumerable<LeaderboardUserDto> Users { get; set; } = Enumerable.Empty<LeaderboardUserDto>();
}
