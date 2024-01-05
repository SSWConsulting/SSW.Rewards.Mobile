namespace SSW.Rewards.Shared.DTOs.Achievements;

public class AchievementUsersViewModel
{
    public string AchievementName { get; set; }

    public IEnumerable<AchievementUserDto> Users { get; set; } = new List<AchievementUserDto>();
}
