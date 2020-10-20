using System;

namespace SSW.Rewards.Domain.Entities
{
    public class UserReward : Entity
    {
        public int UserId { get; set; }
        public User User { get; set; }
        public int RewardId { get; set; }
        public Reward Reward { get; set; }
        public DateTime AwardedAt { get; set; }
    }
}
