using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Application.User.Queries.GetUserRewards
{
    public class UserRewardViewModel
    {
        public string RewardName { get; set; }
        public int RewardCost { get; set; }
        public bool Awarded { get; set; }
        public DateTime? AwardedAt { get; set; }
    }
}
