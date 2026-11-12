using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Infrastructure.Persistence.Configurations;

public class RewardClaimDeliveryConfiguration : IEntityTypeConfiguration<RewardClaimDelivery>
{
    public void Configure(EntityTypeBuilder<RewardClaimDelivery> builder)
    {
        builder
            .HasOne(d => d.UserReward)
            .WithOne()
            .HasForeignKey<RewardClaimDelivery>(d => d.UserRewardId)
            .OnDelete(DeleteBehavior.Cascade);

        // One fulfilment request per claim.
        builder
            .HasIndex(d => d.UserRewardId)
            .IsUnique();

        // Supports the sweep for stale Pending deliveries.
        builder
            .HasIndex(d => d.Status);

        builder.Property(d => d.FreeformAddress).HasMaxLength(500);
        builder.Property(d => d.StreetNumber).HasMaxLength(50);
        builder.Property(d => d.StreetName).HasMaxLength(200);
        builder.Property(d => d.Municipality).HasMaxLength(200);
        builder.Property(d => d.State).HasMaxLength(100);
        builder.Property(d => d.PostalCode).HasMaxLength(20);
        builder.Property(d => d.Country).HasMaxLength(100);
        builder.Property(d => d.LastError).HasMaxLength(1000);
    }
}
