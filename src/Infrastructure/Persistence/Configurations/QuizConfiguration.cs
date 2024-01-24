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

public class QuizQuestionConfiguration : IEntityTypeConfiguration<QuizQuestion>
{
    public void Configure(EntityTypeBuilder<QuizQuestion> builder)
    {
        builder
            .HasMany(x => x.UserAnswers)
            .WithOne(x => x.QuizQuestion)
            .HasForeignKey(x => x.QuizQuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

public class SubmittedAnswerConfiguration : IEntityTypeConfiguration<SubmittedQuizAnswer>
{
    public void Configure(EntityTypeBuilder<SubmittedQuizAnswer> builder)
    {
        builder
            .HasOne(x => x.QuizQuestion)
            .WithMany(x => x.UserAnswers)
            .HasForeignKey(x => x.QuizQuestionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}