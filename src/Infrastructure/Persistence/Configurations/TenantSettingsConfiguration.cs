using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Infrastructure.Persistence.Configurations;

public class TenantSettingsConfiguration : IEntityTypeConfiguration<TenantSettings>
{
    public void Configure(EntityTypeBuilder<TenantSettings> builder)
    {
        builder.ToTable("TenantSettings");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.TenantId).IsRequired();
        builder.HasOne(x => x.Tenant)
            .WithOne(x => x.Settings)
            .HasForeignKey<TenantSettings>(x => x.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Property(x => x.CompanyName).HasMaxLength(200);
        builder.Property(x => x.CompanyLegalName).HasMaxLength(200);
        builder.Property(x => x.CompanyWebsiteUrl).HasMaxLength(300);
        builder.Property(x => x.ApplicationName).HasMaxLength(100);
        builder.Property(x => x.ApplicationShortName).HasMaxLength(50);
        builder.Property(x => x.ApplicationTagline).HasMaxLength(200);
        builder.Property(x => x.LogoUrl).HasMaxLength(300);
        builder.Property(x => x.FaviconUrl).HasMaxLength(300);
        builder.Property(x => x.PrimaryColor).HasMaxLength(20);
        builder.Property(x => x.SecondaryColor).HasMaxLength(20);
        builder.Property(x => x.AccentColor).HasMaxLength(20);
        builder.Property(x => x.BackgroundColor).HasMaxLength(20);
        builder.Property(x => x.TextColor).HasMaxLength(20);
        builder.Property(x => x.SupportEmail).HasMaxLength(200);
        builder.Property(x => x.MarketingEmail).HasMaxLength(200);
        builder.Property(x => x.StaffEmailDomain).HasMaxLength(100);
        builder.Property(x => x.DefaultSenderEmail).HasMaxLength(200);
        builder.Property(x => x.DefaultSenderName).HasMaxLength(100);
        builder.Property(x => x.ProfileDeletionRecipient).HasMaxLength(200);
        builder.Property(x => x.ApiBaseUrl).HasMaxLength(300);
        builder.Property(x => x.IdentityServerUrl).HasMaxLength(300);
        builder.Property(x => x.QuizServiceUrl).HasMaxLength(300);
        builder.Property(x => x.AdminPortalUrl).HasMaxLength(300);
        builder.Property(x => x.LinkedInUrl).HasMaxLength(300);
        builder.Property(x => x.TwitterUrl).HasMaxLength(300);
        builder.Property(x => x.FacebookUrl).HasMaxLength(300);
        builder.Property(x => x.InstagramUrl).HasMaxLength(300);
        builder.Property(x => x.YouTubeUrl).HasMaxLength(300);
    }
}
