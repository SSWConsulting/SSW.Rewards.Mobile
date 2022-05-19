using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Users.Queries.GetUserAchievements
{
    internal class JoinedUserAchievement
    {
        public Domain.Entities.Achievement Achievement { get; set; }
        public UserAchievement UserAchievement { get; set; }
    }
}