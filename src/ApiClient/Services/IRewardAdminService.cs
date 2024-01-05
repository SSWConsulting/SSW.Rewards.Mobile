using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.ApiClient.Services;

public interface IRewardAdminService
{
    Task<RewardsAdminViewModel> GetRewards(CancellationToken cancellationToken);
    Task<int> AddReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task UpdateReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task DeleteReward(int rewardId, CancellationToken cancellationToken);

    Task<ClaimRewardResult> ClaimForUser(string code, int userId, CancellationToken cancellationToken);
}
