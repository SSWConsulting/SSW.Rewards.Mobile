namespace SSW.Rewards.Domain.Entities;

public class PendingRedemption : BaseEntity
{
    public string? Code { get; set; }
    public DateTime ClaimedAt { get; set; }
    public int RewardId { get; set; }
    public Reward Reward { get; set; } = null!;
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    public bool CancelledByUser { get; set; }
    public bool CancelledByAdmin { get; set; }
    public bool Completed { get; set; }
}