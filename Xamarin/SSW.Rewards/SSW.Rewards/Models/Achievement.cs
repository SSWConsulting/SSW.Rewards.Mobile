using System;

namespace SSW.Rewards.Models
{
    public class Achievement
    {
        public string Name { get; set; }
        public int Value { get; set; }
        public bool Complete { get; set; }
        public AchievementType Type { get; set; }
        public DateTime? AwardedAt { get; set; }
    }
}
