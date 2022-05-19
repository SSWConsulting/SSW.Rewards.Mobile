using System;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards
{
    public class UserRewardViewModel
    {
        public string RewardName { get; set; }
        public int RewardCost { get; set; }
        public bool Awarded { get; set; }
        public DateTime? AwardedAt { get; set; }
    }
}
