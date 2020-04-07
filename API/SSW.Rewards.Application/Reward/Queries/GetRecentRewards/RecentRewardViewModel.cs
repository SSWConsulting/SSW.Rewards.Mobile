using System;

namespace SSW.Rewards.Application.Reward.Queries.GetRecentRewards
{
    public class RecentRewardViewModel
    {
        public string RewardName { get; set; }
        public int RewardCost { get; set; }
        public string AwardedTo { get; set; }
        public DateTime AwardedAt { get; set; }
    }
}
