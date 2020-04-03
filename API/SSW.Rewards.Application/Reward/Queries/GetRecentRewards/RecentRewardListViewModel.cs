using System.Collections.Generic;

namespace SSW.Rewards.Application.Reward.Queries.GetRecentRewards
{
    public class RecentRewardListViewModel
    {
        public IEnumerable<RecentRewardViewModel> Rewards { get; set; }
    }
}
