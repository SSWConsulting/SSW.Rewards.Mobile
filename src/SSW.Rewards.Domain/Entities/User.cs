namespace SSW.Rewards.Domain.Entities;
public class User : BaseEntity
{
    public string? FullName { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Avatar { get; set; } = string.Empty;
    public PostalAddress Address { get; set; } = new();
    public int? AddressId { get; set; }
    public bool Activated { get; set; }
    public ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();
    public ICollection<UserReward> UserRewards { get; set; } = new HashSet<UserReward>();
    public ICollection<UserRole> Roles { get; set; } = new HashSet<UserRole>();
    public ICollection<Notification> SentNotifications { get; set; } = new HashSet<Notification>();
    public ICollection<CompletedQuiz> CompletedQuizzes { get; set; } = new HashSet<CompletedQuiz>();
    public ICollection<UserSocialMediaId> SocialMediaIds { get; set; } = new HashSet<UserSocialMediaId>();
}
