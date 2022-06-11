using SSW.Rewards.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.Services
{
    public class RewardService : BaseService, IRewardService
    {
        private RewardClient _rewardClient;

        public RewardService()
        {
            _rewardClient = new RewardClient(BaseUrl, AuthenticatedClient);
        }

        public async Task<List<Reward>> GetRewards()
        {
            var rewards = await _rewardClient.ListAsync();

            var rewardList = new List<Reward>();

            foreach (var reward in rewards.Rewards)
            {
                rewardList.Add(new Reward
                {
                    Cost = reward.Cost,
                    Id = reward.Id,
                    ImageUri = reward.ImageUri,
                    Name = reward.Name
                });
            }

            return rewardList;
        }

        public async Task<ClaimRewardResult> RedeemReward(Reward reward)
        {
            throw new NotImplementedException();
        }

        public async Task<ClaimRewardResult> RewardRewardWithQRCode(string QRCode)
        {
            throw new NotImplementedException();
        }
    }
}
