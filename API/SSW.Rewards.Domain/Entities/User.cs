using System.Collections.Generic;

namespace SSW.Rewards.Domain.Entities
{
    public class User : Entity
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Avatar { get; set; }
        public PostalAddress? Address { get; set; }
        public int? AddressId { get; set; }
        public ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();
        public ICollection<UserReward> UserRewards { get; set; } = new HashSet<UserReward>();
    }
}


