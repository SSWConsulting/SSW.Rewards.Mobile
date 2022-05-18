using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards
{
    internal class JoinedUserReward
    {
        public Domain.Entities.Reward Reward { get; set; }
        public UserReward UserReward { get; set; }
    }
}
