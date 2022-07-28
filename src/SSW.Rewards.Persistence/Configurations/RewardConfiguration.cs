using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Persistence.Configurations
{
    public class RewardConfiguration : IEntityTypeConfiguration<Reward>
    {
        public void Configure(EntityTypeBuilder<Reward> builder)
        {
        }
    }
}
