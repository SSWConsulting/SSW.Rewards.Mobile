namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementUsers;

public class AchievementUsersViewModel
{
    public string AchievementName { get; set; }
    
    public IEnumerable<AchievementUserDto> Users { get; set; } = new List<AchievementUserDto>();
}