using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Common;

namespace SSW.Rewards.Infrastructure.Persistence.Configurations;

internal static class ConfigurationExtensions
{
    public static void HasSoftDelete<TEntity>(this EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseAuditableEntity
    {
        // We are mostly interested only in data that has NULL value. Non-null values will be rare.
        builder
            .HasIndex(x => x.DeletedUtc)
            .HasFilter("[DeletedUtc] IS NULL");

        builder.HasQueryFilter(e => e.DeletedUtc == null);
    }
}
