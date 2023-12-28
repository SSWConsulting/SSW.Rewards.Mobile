using Shared.DTOs.Rewards;

namespace Shared.Interfaces;

public interface IRewardAdminService
{
    Task<int> AddReward(RewardEditDto reward);
    Task UpdateReward(RewardEditDto reward);

    Task<ClaimRewardResult> ClaimForUser(string code, int userId);
}
