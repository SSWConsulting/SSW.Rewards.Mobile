using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Persistence.Configurations
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
                .Property(ur => ur.AwardedAt)
                .HasDefaultValueSql("getdate()")
                .ValueGeneratedOnAdd();
        }
    }
}
