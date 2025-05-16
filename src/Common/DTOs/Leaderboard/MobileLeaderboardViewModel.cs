using SSW.Rewards.Shared.DTOs.Pagination;

namespace SSW.Rewards.Shared.DTOs.Leaderboard;

public class MobileLeaderboardViewModel : IPaginationResult<MobileLeaderboardUserDto>
{
    public IEnumerable<MobileLeaderboardUserDto> Items { get; set; } = [];

    public LeaderboardFilter CurrentPeriod { get; set; }

    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Count { get; set; }
}

public class MobileLeaderboardUserDto
{
    public int Rank { get; set; }
    public int UserId { get; set; }
    public string? Name { get; set; }
    public string? ProfilePic { get; set; }
    public int Points { get; set; }
}
