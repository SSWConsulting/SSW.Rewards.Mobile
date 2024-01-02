using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Persistence.Configurations;

public class QuizConfiguration : IEntityTypeConfiguration<Quiz>
{
    public void Configure(EntityTypeBuilder<Quiz> builder)
    {
        builder
            .HasOne(q => q.CreatedBy)
            .WithMany(c => c.CreatedQuizzes)
            .OnDelete(DeleteBehavior.NoAction);
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
