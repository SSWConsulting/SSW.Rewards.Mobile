using System;

namespace SSW.Rewards.Application.Users.Queries.GetUserAchievements
{
    public class UserAchievementDto
    {
        public string AchievementName { get; set; }
        public int AchievementValue { get; set; }
        public bool Complete { get; set; }
        public AchievementType AchievementType { get; set; }
        public DateTime? AwardedAt { get; set; }
    }
}
