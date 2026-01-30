
namespace SSW.Rewards.Domain.Entities;

public class TenantSettings
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public Tenant Tenant { get; set; } = null!;

    // Branding
    public string CompanyName { get; set; } = string.Empty;
    public string CompanyLegalName { get; set; } = string.Empty;
    public string CompanyWebsiteUrl { get; set; } = string.Empty;
    public string ApplicationName { get; set; } = string.Empty;
    public string ApplicationShortName { get; set; } = string.Empty;
    public string ApplicationTagline { get; set; } = string.Empty;
    public string LogoUrl { get; set; } = string.Empty;
    public string FaviconUrl { get; set; } = string.Empty;

    // Colors
    public string PrimaryColor { get; set; } = "#CC4141";
    public string SecondaryColor { get; set; } = "#333333";
    public string AccentColor { get; set; } = "#CC4141";
    public string BackgroundColor { get; set; } = "#FFFFFF";
    public string TextColor { get; set; } = "#000000";

    // Contact
    public string SupportEmail { get; set; } = string.Empty;
    public string MarketingEmail { get; set; } = string.Empty;
    public string StaffEmailDomain { get; set; } = string.Empty;
    public string DefaultSenderEmail { get; set; } = string.Empty;
    public string DefaultSenderName { get; set; } = string.Empty;
    public string ProfileDeletionRecipient { get; set; } = string.Empty;

    // External Services
    public string ApiBaseUrl { get; set; } = string.Empty;
    public string IdentityServerUrl { get; set; } = string.Empty;
    public string QuizServiceUrl { get; set; } = string.Empty;
    public string AdminPortalUrl { get; set; } = string.Empty;

    // Social Media
    public string LinkedInUrl { get; set; } = string.Empty;
    public string TwitterUrl { get; set; } = string.Empty;
    public string FacebookUrl { get; set; } = string.Empty;
    public string InstagramUrl { get; set; } = string.Empty;
    public string YouTubeUrl { get; set; } = string.Empty;
}
