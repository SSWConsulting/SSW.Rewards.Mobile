using System;

namespace SSW.Rewards.Application.Users.Queries.GetUserAchievements
{
    public class UserAchievementViewModel
    {
        public string AchievementName { get; set; }
        public int AchievementValue { get; set; }
        public bool Complete { get; set; }
        public DateTime? AwardedAt { get; set; }
    }
}
