namespace Shared.DTOs.Achievements;

public class AchievementUserDto
{
    public int UserId { get; set; }

    public string UserName { get; set; }

    public string UserEmail { get; set; }

    public DateTime AwardedAtUtc { get; set; }
}
