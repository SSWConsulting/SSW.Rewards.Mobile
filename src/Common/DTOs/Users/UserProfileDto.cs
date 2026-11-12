namespace SSW.Rewards.Shared.DTOs.Users;
public class UserProfileDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public bool IsStaff { get; set; }

    /// <summary>
    /// Display title: staff record title, else Company link, else "Community". Same as the Activity feed and leaderboard.
    /// </summary>
    public string Title { get; set; } = string.Empty;
    public string? ProfilePic { get; set; }
    public int Points { get; set; }
    public int Balance { get; set; }
    public int Rank { get; set; }
    public IEnumerable<UserRewardDto> Rewards { get; set; } = [];
    public IEnumerable<UserAchievementDto> Achievements { get; set; } = [];
}
