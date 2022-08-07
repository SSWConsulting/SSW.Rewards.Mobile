using SSW.Rewards.Application.Rewards.Common;

namespace SSW.Rewards.Application.Rewards.Commands;

public class ClaimRewardResult
{
    public RewardViewModel viewModel { get; set; }
    public RewardStatus status { get; set; }
}

public enum RewardStatus
{
    Claimed,
    NotFound,
    Duplicate,
    NotEnoughPoints,
    Error
}
