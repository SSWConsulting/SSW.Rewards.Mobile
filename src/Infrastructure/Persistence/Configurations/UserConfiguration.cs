using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder
            .HasIndex(u => u.Email);

        builder
            .Property(u => u.Email)
            .HasConversion(
                v => v.ToLowerInvariant(), 
                v => v);

        builder
            .HasMany(u => u.CreatedQuizzes)
            .WithOne(q => q.CreatedBy)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasMany(x => x.DeviceTokens)
            .WithOne(x => x.User)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
