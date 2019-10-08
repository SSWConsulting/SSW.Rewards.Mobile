using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Persistence.Configurations
{
    public class UserAchievementConfiguration : IEntityTypeConfiguration<UserAchievement>
    {
        public void Configure(EntityTypeBuilder<UserAchievement> builder)
        {
            builder
                .HasOne(ua => ua.User)
                .WithMany(u => u.UserAchievements);

            builder
                .HasOne(ua => ua.Achievement)
                .WithMany(u => u.UserAchievements);

            builder
                .Property(ua => ua.AwardedAt)
                .HasDefaultValueSql("getdate()")
                .ValueGeneratedOnAdd();
        }
    }
}
