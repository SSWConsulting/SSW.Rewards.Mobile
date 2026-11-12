namespace SSW.Rewards.Application.Common.Models;

/// <summary>
/// Everything Marketing needs to fulfil a claimed reward, loaded from the delivery outbox record.
/// </summary>
public record RewardClaimNotification(
    int ClaimId,
    string UserName,
    string UserEmail,
    string RewardName,
    RewardType RewardType,
    string Address);
