namespace SSW.Rewards.Domain.Entities
{
    public class StaffMemberSkill : Entity
    {
        public int SkillId { get; set; }
        public Skill Skill { get; set; }
        public int StaffMemberId { get; set; }
        public StaffMember StaffMember { get; set; }
        public SkillLevel Level { get; set; }
    }

    public enum SkillLevel
    {
        Beginner,
        Intermediate,
        Advanced
    }
}


