using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Persistence.Configurations
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
                .HasIndex(ua => new { ua.UserId, ua.AchievementId })
                .IsUnique();

            builder
                .Property(ua => ua.AwardedAt)
                .HasDefaultValueSql("getdate()")
                .ValueGeneratedOnAdd();
        }
    }
}
