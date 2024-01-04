using Shared.DTOs.Rewards;

namespace SSW.Rewards.Mobile.Services;

public interface IRewardService
{
    Task<RewardListViewModel> GetRewards(CancellationToken cancellationToken);
    Task<RewardListViewModel?> GetOnboardingRewards(CancellationToken cancellationToken);
    Task<RecentRewardListViewModel> GetRecentReward(DateTime? since, CancellationToken cancellationToken);
    Task<RewardListViewModel> SearchRewards(string searchTerm, CancellationToken cancellationToken);


    Task<ClaimRewardResult> RedeemReward(string code, bool inPerson, CancellationToken cancellationToken);
    Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode, CancellationToken cancellationToken);
}
