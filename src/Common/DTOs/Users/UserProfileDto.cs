namespace SSW.Rewards.Shared.DTOs.Users;
public class UserProfileDto
{
    public int Id { get; set; }
    public string? FullName { get; set; }
    public string? Email { get; set; }
    public bool IsStaff { get; set; }
    public string? ProfilePic { get; set; }
    public int Points { get; set; }
    public int Balance { get; set; }
    public int Rank { get; set; }
    public IEnumerable<UserRewardDto> Rewards { get; set; } = Enumerable.Empty<UserRewardDto>();
    public IEnumerable<UserAchievementDto> Achievements { get; set; } = Enumerable.Empty<UserAchievementDto>();
}
