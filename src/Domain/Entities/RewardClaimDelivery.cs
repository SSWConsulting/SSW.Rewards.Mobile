namespace SSW.Rewards.Domain.Entities;

/// <summary>
/// Outbox record for a claimed reward that must be fulfilled by SSW Marketing.
/// Saved in the same transaction as the <see cref="UserReward"/> so the fulfilment request
/// (including the shipping address) survives email failures and can be resumed from the database.
/// </summary>
public class RewardClaimDelivery : BaseEntity
{
    public int UserRewardId { get; set; }
    public UserReward UserReward { get; set; } = null!;

    public string? FreeformAddress { get; set; }
    public string? StreetNumber { get; set; }
    public string? StreetName { get; set; }
    public string? Municipality { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }

    public RewardClaimDeliveryStatus Status { get; set; } = RewardClaimDeliveryStatus.Pending;
    public int Attempts { get; set; }
    public string? LastError { get; set; }
    public DateTime? LastAttemptUtc { get; set; }
    public DateTime? DeliveredUtc { get; set; }
}
