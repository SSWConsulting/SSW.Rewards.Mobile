namespace SSW.Rewards.Domain.Entities;

public class PendingRedemption : BaseEntity
{
    public string? Code { get; set; }
    public DateTime ClaimedAt { get; set; }
    public Reward Reward { get; set; } = null!;
    public User User { get; set; } = null!;
    public bool CancelledByUser { get; set; }
    public bool CancelledByAdmin { get; set; }
}