using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Infrastructure.Persistence.Configurations;

public class OpenProfileDeletionRequestConfiguration : IEntityTypeConfiguration<OpenProfileDeletionRequest>
{
    public void Configure(EntityTypeBuilder<OpenProfileDeletionRequest> builder)
    {
        builder.HasSoftDelete();
    }
}
