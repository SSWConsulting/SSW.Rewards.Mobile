using System.Collections.Generic;

namespace SSW.Rewards.Domain.Entities
{
    public class Role : Entity
    {
        public string Name { get; set; }

        public ICollection<UserRole> Users { get; set; } = new HashSet<UserRole>();
    }
}
