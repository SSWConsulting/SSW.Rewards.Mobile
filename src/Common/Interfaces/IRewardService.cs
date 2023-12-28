using Shared.DTOs.Rewards;

namespace SSW.Rewards.Mobile.Services;

public interface IRewardService
{
    Task<RewardListViewModel> GetRewards();
    Task<RewardListViewModel?> GetOnboardingRewards();
    Task<RecentRewardListViewModel> GetRecentReward(DateTime? since);
    Task<RewardListViewModel> SearchRewards(string searchTerm);


    Task<ClaimRewardResult> RedeemReward(string code, bool inPerson);
    Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode);
}
