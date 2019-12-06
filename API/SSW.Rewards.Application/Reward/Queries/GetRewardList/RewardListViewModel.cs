using System.Collections.Generic;

namespace SSW.Rewards.Application.Reward.Queries.GetRewardList
{
    public class RewardListViewModel
    {
        public IEnumerable<RewardViewModel> Rewards { get; set; }
    }
}
