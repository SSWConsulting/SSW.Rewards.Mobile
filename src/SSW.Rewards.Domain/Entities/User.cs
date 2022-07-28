using System.Collections.Generic;

namespace SSW.Rewards.Domain.Entities
{
    public class User : Entity
    {
        public string FullName { get; set; }
        
        public string Email { get; set; }
        
        public string Avatar { get; set; }
        
        public PostalAddress Address { get; set; }

        public int? AddressId { get; set; }

        public bool Activated { get; set; }
        
        public ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();

        public ICollection<UserReward> UserRewards { get; set; } = new HashSet<UserReward>();

        public ICollection<UserRole> Roles { get; set; } = new HashSet<UserRole>();
        
        public ICollection<Notifications> SentNotifications { get; set; } = new HashSet<Notifications>();
        public ICollection<CompletedQuiz> CompletedQuizzes { get; set; } = new HashSet<CompletedQuiz>();
    }
}


