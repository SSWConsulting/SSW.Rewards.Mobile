using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Persistence.Configurations
{
    public class StaffMemberSkillConfiguration : IEntityTypeConfiguration<StaffMemberSkill>
    {
        public void Configure(EntityTypeBuilder<StaffMemberSkill> builder)
        {
            builder
                .HasIndex(sms => new { sms.SkillId, sms.StaffMemberId })
                .IsUnique();

            builder
                .HasOne(bc => bc.StaffMember)
                .WithMany(c => c.StaffMemberSkills);

            builder
                .HasOne(bc => bc.Skill);
        }
    }
}
