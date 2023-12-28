using Shared.Models;

namespace Shared.DTOs.Rewards;

public class ClaimRewardResult
{
    public RewardDto? Reward { get; set; }
    public RewardStatus status { get; set; }
}
