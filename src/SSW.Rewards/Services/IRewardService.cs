using SSW.Rewards.Models;

namespace SSW.Rewards.Services;

public interface IRewardService
{
    Task<List<Reward>> GetRewards();
    Task<ClaimRewardResult> RedeemReward(Reward reward);
    Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode);
}
