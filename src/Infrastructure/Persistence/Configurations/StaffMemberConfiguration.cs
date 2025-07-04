using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence.Configurations;

namespace SSW.Rewards.Persistence.Configurations;

public class StaffMemberConfiguration : IEntityTypeConfiguration<StaffMember>
{
    public void Configure(EntityTypeBuilder<StaffMember> builder)
    {
        builder.Property(s => s.IsExternal)
            .HasDefaultValue(false);

        builder.HasSoftDelete();
    }
}
