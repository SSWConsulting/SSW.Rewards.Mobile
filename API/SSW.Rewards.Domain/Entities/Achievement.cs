using System.Collections.Generic;

namespace SSW.Rewards.Domain.Entities
{
    public class Achievement : Entity
    {
        public string Code { get; set; }

        public string Name { get; set; }
        
        public int Value { get; set; }

        public AchievementType Type { get; set; }

        public Icons Icon { get; set; }

        public bool IconIsBranded { get; set; }

        public ICollection<UserAchievement> UserAchievements { get; set; } = new HashSet<UserAchievement>();
    }
}


