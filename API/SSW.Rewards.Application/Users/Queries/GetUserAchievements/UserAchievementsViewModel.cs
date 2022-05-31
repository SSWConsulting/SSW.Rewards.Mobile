using System.Collections.Generic;

namespace SSW.Rewards.Application.Users.Queries.GetUserAchievements
{
    public class UserAchievementsViewModel
    {
        public int UserId { get; set; }
        public int Points { get; set; }

        public IEnumerable<UserAchievementDto> UserAchievements { get; set; }
    }
}
