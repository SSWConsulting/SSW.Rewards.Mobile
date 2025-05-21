using System.Text.Json.Serialization;
using SSW.Rewards.Shared.DTOs.Pagination;

namespace SSW.Rewards.Shared.DTOs.Leaderboard;

public class MobileLeaderboardViewModel : PaginationResult<MobileLeaderboardUserDto>
{
    public LeaderboardFilter CurrentPeriod { get; set; }
}

public class MobileLeaderboardUserDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? ProfilePic { get; set; }
    public int Points { get; set; }
}
