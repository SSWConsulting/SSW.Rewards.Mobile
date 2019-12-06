using System.Collections.Generic;

namespace SSW.Rewards.Application.User.Queries.GetUserRewards
{
    public class UserRewardsViewModel
    {
        public int UserId { get; set; }
        public IEnumerable<UserRewardViewModel> UserRewards { get; set; }
    }
}
