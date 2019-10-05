using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Persistence.Configurations
{
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
        }
    }
}
