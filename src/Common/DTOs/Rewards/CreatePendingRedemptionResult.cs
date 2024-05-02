using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.Shared.DTOs.Rewards;

public class CreatePendingRedemptionResult
{
    public RewardDto? Reward { get; set; }
    public string Code { get; set; } = string.Empty;
    public RewardStatus status { get; set; }
}
