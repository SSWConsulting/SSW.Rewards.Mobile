namespace Shared.DTOs.Users;

public class UserAchievementsViewModel
{
    public int UserId { get; set; }
    public int Points { get; set; }

    public IEnumerable<UserAchievementDto> UserAchievements { get; set; } = Enumerable.Empty<UserAchievementDto>();
}
