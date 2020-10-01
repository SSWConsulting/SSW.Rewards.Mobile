using SSW.Rewards.Application.Reward.Queries.Common;

namespace SSW.Rewards.Application.Reward.Commands
{
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
}
