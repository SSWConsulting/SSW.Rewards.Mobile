using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Application.User.Queries.GetUserAchievements
{
    internal class JoinedUserAchievement
    {
        public Domain.Entities.Achievement Achievement { get; set; }
        public UserAchievement UserAchievement { get; set; }
    }
}