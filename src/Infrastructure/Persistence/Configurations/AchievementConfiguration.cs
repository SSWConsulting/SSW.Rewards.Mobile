﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence.Configurations;

namespace SSW.Rewards.Persistence.Configurations;

public class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> builder)
    {
        builder.HasIndex(a => a.IntegrationId)
            .IsUnique();

        builder
            .HasIndex(x => x.Id)
            .IncludeProperties(x => x.Value);

        builder.HasSoftDelete();
    }
}
