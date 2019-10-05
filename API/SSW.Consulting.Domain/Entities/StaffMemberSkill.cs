namespace SSW.Consulting.Domain.Entities
{
    public class StaffMemberSkill : Entity
    {
        public int SkillId { get; set; }
        public Skill Skill { get; set; }
        public int StaffMemberId { get; set; }
        public StaffMember StaffMember { get; set; }
    }
}


