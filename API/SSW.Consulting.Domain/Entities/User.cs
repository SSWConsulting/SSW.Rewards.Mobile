using System.Collections.Generic;

namespace SSW.Consulting.Domain.Entities
{
    public class User : Entity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();
        public ICollection<UserReward> UserRewards { get; set; } = new HashSet<UserReward>();
    }
}


