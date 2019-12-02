using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Application.User.Queries.GetUserRewards
{
    internal class JoinedUserReward
    {
        public Domain.Entities.Reward Reward { get; set; }
        public UserReward UserReward { get; set; }
    }
}
