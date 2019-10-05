using System.Collections.Generic;

namespace SSW.Consulting.Domain.Entities
{
    public class StaffMember : Entity
    {
        public string Name { get; set; }
        public string Title { get; set; }
        public string Email { get; set; }
        public string Profile { get; set; }
        public string TwitterUsername { get; set; }
        public ICollection<StaffMemberSkill> StaffMemberSkills { get; set; } = new HashSet<StaffMemberSkill>();
    }
}


