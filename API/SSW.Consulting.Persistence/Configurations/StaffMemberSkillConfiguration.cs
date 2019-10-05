using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Persistence.Configurations
{
    public class StaffMemberSkillConfiguration : IEntityTypeConfiguration<StaffMemberSkill>
    {
        public void Configure(EntityTypeBuilder<StaffMemberSkill> builder)
        {
            builder
                .HasOne(bc => bc.StaffMember)
                .WithMany(c => c.StaffMemberSkills);

            builder
                .HasOne(bc => bc.Skill);
        }
    }
}
