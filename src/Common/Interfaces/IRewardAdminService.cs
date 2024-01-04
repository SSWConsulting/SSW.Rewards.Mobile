using Shared.DTOs.Rewards;

namespace Shared.Interfaces;

public interface IRewardAdminService
{
    Task<int> AddReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task UpdateReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task DeleteReward(int rewardId, CancellationToken cancellationToken);

    Task<ClaimRewardResult> ClaimForUser(string code, int userId, CancellationToken cancellationToken);
}
