using SSW.Rewards.Models;
using SSW.Rewards.ViewModels;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public interface IRewardService
    {
        Task<List<Reward>> GetRewards();
        Task<ClaimRewardResult> RedeemReward(Reward reward);
        Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode);
    }
}
