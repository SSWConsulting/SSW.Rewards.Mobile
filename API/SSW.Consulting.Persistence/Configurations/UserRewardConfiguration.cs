using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Consulting.Domain.Entities;

namespace SSW.Consulting.Persistence.Configurations
{
    public class UserRewardConfiguration : IEntityTypeConfiguration<UserReward>
    {
        public void Configure(EntityTypeBuilder<UserReward> builder)
        {
            builder
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRewards);

            builder
                .HasOne(ur => ur.Reward)
                .WithMany(u => u.UserRewards);

            builder
                .HasIndex(ur => new { ur.UserId, ur.RewardId })
                .IsUnique();

            builder
                .Property(ur => ur.AwardedAt)
                .HasDefaultValueSql("getdate()")
                .ValueGeneratedOnAdd();
        }
    }
}
