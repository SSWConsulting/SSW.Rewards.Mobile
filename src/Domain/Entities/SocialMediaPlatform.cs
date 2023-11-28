namespace SSW.Rewards.Domain.Entities;
public class SocialMediaPlatform : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public ICollection<UserSocialMediaId> UserSocialMediaIds { get; set; } = new HashSet<UserSocialMediaId>();
    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; } = null!;
}
