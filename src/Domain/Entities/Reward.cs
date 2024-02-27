namespace SSW.Rewards.Domain.Entities;
public class Reward : BaseEntity
{
    public string? Code { get; set; } = string.Empty;
    public string? Name { get; set; } = string.Empty;
    public int Cost { get; set; }
    public string? ImageUri { get; set; } = string.Empty;
    public string? CarouselImageUri { get; set; } = string.Empty;
    public bool IsCarousel { get; set; } = false;
    public RewardType RewardType { get; set; } = RewardType.Digital;
    public Icons Icon { get; set; }
    public bool IconIsBranded { get; set; }
    public bool IsOnboardingReward { get; set; }
    public virtual ICollection<UserReward> UserRewards { get; set; } = new HashSet<UserReward>();
}
