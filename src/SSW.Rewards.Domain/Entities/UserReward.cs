namespace SSW.Rewards.Domain.Entities;
public class UserReward : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = new();
    public int RewardId { get; set; }
    public Reward Reward { get; set; } = new();
    public DateTime AwardedAt { get; set; }
}
