using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Persistence.Configurations
{
    public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
    {
        public void Configure(EntityTypeBuilder<Quiz> builder)
        {
        }
    }

    public class QuizAnswerConfiguration : IEntityTypeConfiguration<QuizAnswer>
    {
        public void Configure(EntityTypeBuilder<QuizAnswer> builder)
        {
            builder
                .HasOne(x => x.Question)
                .WithMany(a => a.Answers)
                .HasForeignKey(x => x.QuestionId);
        }
    }
}
