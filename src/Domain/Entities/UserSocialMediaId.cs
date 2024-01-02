namespace SSW.Rewards.Domain.Entities;
public class UserSocialMediaId : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; }
    public int SocialMediaPlatformId { get; set; }
    public SocialMediaPlatform SocialMediaPlatform { get; set; }
    public string SocialMediaUserId { get; set; }
}
