namespace SSW.Rewards.Mobile.Services;

public interface IRewardService
{
    Task<List<Reward>> GetRewards();
    Task<ClaimRewardResult> RedeemReward(Reward reward);
    Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode);
}
