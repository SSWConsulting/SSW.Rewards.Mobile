using System;

namespace SSW.Rewards.Models
{
    public class Achievement
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int Value { get; set; }

        public bool Complete { get; set; }
        
        public AchievementType Type { get; set; }

        public Icons AchievementIcon { get; set; }

        public bool IconIsBranded { get; set; }

        public DateTime? AwardedAt { get; set; }
    }
}
