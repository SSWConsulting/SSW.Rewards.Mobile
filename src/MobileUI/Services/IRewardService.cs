using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Mobile.Services;

public interface IRewardService
{
    Task<List<Reward>> GetRewards();
    Task <ClaimRewardResult> ClaimReward(ClaimRewardDto claim);
    Task<CreatePendingRedemptionResult> CreatePendingRedemption(CreatePendingRedemptionDto claim);
    Task<CancelPendingRedemptionResult> CancelPendingRedemption(CancelPendingRedemptionDto claim);
    Task<ClaimRewardResult> ClaimRewardForUser(ClaimRewardDto claim);
}
