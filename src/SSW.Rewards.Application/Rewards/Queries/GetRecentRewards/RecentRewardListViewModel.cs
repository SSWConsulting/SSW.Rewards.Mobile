using System.Collections.Generic;

namespace SSW.Rewards.Application.Rewards.Queries.GetRecentRewards
{
    public class RecentRewardListViewModel
    {
        public IEnumerable<RecentRewardViewModel> Rewards { get; set; }
    }
}
