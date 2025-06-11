using System.Text.Json.Serialization;
using SSW.Rewards.Shared.DTOs.Pagination;

namespace SSW.Rewards.Shared.DTOs.Leaderboard;

public class MobileLeaderboardViewModel : PaginationResult<MobileLeaderboardUserDto>
{
    public LeaderboardFilter CurrentPeriod { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int MyRank { get; set; }
}

public class MobileLeaderboardUserDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? Title { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public string? ProfilePic { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public int Points { get; set; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingDefault)]
    public bool IsMe { get; set; }
}
