using Shared.DTOs.Rewards;

namespace SSW.Rewards.Mobile.Services;

public interface IRewardService
{
    Task<RewardListViewModel> GetRewards();
    Task<ClaimRewardResult> RedeemReward(string code, bool inPerson);
    Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode);
}
