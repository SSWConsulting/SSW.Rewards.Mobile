namespace SSW.Rewards.Domain.Entities;
public class Achievement : BaseEntity
{
    public string? Code { get; set; }

    public string? Name { get; set; }

    public int Value { get; set; }

    public AchievementType Type { get; set; }

    public Icons Icon { get; set; }

    public bool IconIsBranded { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsMultiscanEnabled { get; set; }

    public ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();
    public ICollection<Quiz> Quizzes { get; set; } = new HashSet<Quiz>();
    public ICollection<SocialMediaPlatform> SocialMediaPlatforms { get; set; } = new HashSet<SocialMediaPlatform>();
}
