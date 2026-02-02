using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Infrastructure.Persistence.Configurations;

namespace SSW.Rewards.Persistence.Configurations;

public class PostCommentConfiguration : IEntityTypeConfiguration<PostComment>
{
    public void Configure(EntityTypeBuilder<PostComment> builder)
    {
        builder.Property(pc => pc.Comment)
            .IsRequired()
            .HasMaxLength(1000);

        builder.HasOne(pc => pc.Post)
            .WithMany(p => p.PostComments)
            .HasForeignKey(pc => pc.PostId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(pc => pc.User)
            .WithMany(u => u.PostComments)
            .HasForeignKey(pc => pc.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(pc => pc.PostId);
        builder.HasIndex(pc => pc.UserId);

        builder.HasSoftDelete();
    }
}
