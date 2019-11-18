using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting.Domain.Entities
{
    public class Reward : Entity
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Cost { get; set; }

        public ICollection<UserReward> UserRewards { get; set; } = new HashSet<UserReward>();
    }
}
