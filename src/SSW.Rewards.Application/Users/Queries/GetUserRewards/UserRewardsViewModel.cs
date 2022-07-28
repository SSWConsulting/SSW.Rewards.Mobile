using System.Collections.Generic;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards
{
    public class UserRewardsViewModel
    {
        public int UserId { get; set; }
        public IEnumerable<UserRewardDto> UserRewards { get; set; } = new List<UserRewardDto>();
    }
}
