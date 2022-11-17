namespace SSW.Rewards.Domain.Entities;
public class UserReward : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public int RewardId { get; set; }
    public Reward Reward { get; set; } = null!;
    public DateTime AwardedAt { get; set; }
}
