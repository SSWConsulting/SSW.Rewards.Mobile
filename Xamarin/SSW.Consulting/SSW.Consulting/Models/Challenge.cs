using System;
namespace SSW.Consulting.Models
{
    public class Challenge
    {
        public Challenge()
        {
        }

        public int id { get; set; }
        public string Title { get; set; }
        public string Badge { get; set; }
        public int Points { get; set; }
        public string Picture { get; set; }
        public bool IsBonus { get; set; }
        public ChallengeType ChallengeType { get; set; }
        public DateTimeOffset? AwardedAt { get; set; }
    }
}
