using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Infrastructure.Persistence.Configurations;
public class SubmittedQuizAnswerConfiguration : IEntityTypeConfiguration<SubmittedQuizAnswer>
{
    public void Configure(EntityTypeBuilder<SubmittedQuizAnswer> builder)
    {
        builder.HasOne(s => s.Answer)
            .WithMany(a => a.SubmittedAnswers)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
