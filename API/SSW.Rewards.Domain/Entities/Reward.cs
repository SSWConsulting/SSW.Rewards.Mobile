using System.Collections.Generic;

namespace SSW.Rewards.Domain.Entities
{
    public class Reward : Entity
    {
        public string Code { get; set; }
        
        public string Name { get; set; }
        
        public int Cost { get; set; }
        
        public string ImageUri { get; set; }

        public RewardType RewardType { get; set; }

        public Icons Icon { get; set; }

        public bool IconIsBranded { get; set; }

        public ICollection<UserReward> UserRewards { get; set; } = new HashSet<UserReward>();
    }
}
