using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards;

internal class JoinedUserReward
{
    Reward Reward { get; set; }
    public UserReward UserReward { get; set; }
}
