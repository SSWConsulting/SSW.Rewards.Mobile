using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Mobile.Services;

public interface IRewardService
{
    Task<List<Reward>> GetRewards();
    Task ClaimReward(ClaimRewardDto claim);
}
