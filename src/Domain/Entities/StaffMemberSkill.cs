namespace SSW.Rewards.Domain.Entities;
public class StaffMemberSkill : BaseEntity
{
    public int SkillId { get; set; }
    public Skill Skill { get; set; } = null!;
    public int StaffMemberId { get; set; }
    public StaffMember StaffMember { get; set; } = null!;
    public SkillLevel Level { get; set; }
}
