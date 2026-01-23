namespace SSW.Rewards.Shared.Configuration;

/// <summary>
/// Tenant-specific configuration for white-labeling the SSW Rewards platform.
/// This class consolidates all tenant-specific settings in one place to enable
/// easy deployment of branded instances for different clients.
/// </summary>
public class TenantSettings
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string SectionName = "TenantSettings";

    /// <summary>
    /// Branding and company information
    /// </summary>
    public BrandingSettings Branding { get; set; } = new();

    /// <summary>
    /// Color scheme and theme settings
    /// </summary>
    public ColorSettings Colors { get; set; } = new();

    /// <summary>
    /// Contact information and email settings
    /// </summary>
    public ContactSettings Contact { get; set; } = new();

    /// <summary>
    /// External service URLs and integrations
    /// </summary>
    public ExternalServicesSettings ExternalServices { get; set; } = new();

    /// <summary>
    /// Social media URLs and integrations
    /// </summary>
    public SocialMediaSettings SocialMedia { get; set; } = new();

    /// <summary>
    /// Branding and company identity settings
    /// </summary>
    public class BrandingSettings
    {
        /// <summary>
        /// Company name (e.g., "SSW", "ACME Corp")
        /// </summary>
        public string CompanyName { get; set; } = string.Empty;

        /// <summary>
        /// Legal company name for official documents (e.g., "Superior Software for Windows Pty Ltd")
        /// </summary>
        public string CompanyLegalName { get; set; } = string.Empty;

        /// <summary>
        /// Company website URL (e.g., "https://www.ssw.com.au")
        /// </summary>
        public string CompanyWebsiteUrl { get; set; } = string.Empty;

        /// <summary>
        /// Application display name (e.g., "SSW Rewards")
        /// </summary>
        public string ApplicationName { get; set; } = string.Empty;

        /// <summary>
        /// Short application name for limited space (e.g., "SSW Rewards")
        /// </summary>
        public string ApplicationShortName { get; set; } = string.Empty;

        /// <summary>
        /// Application tagline or slogan (e.g., "Earn rewards, connect with SSW")
        /// </summary>
        public string ApplicationTagline { get; set; } = string.Empty;

        /// <summary>
        /// URL to the company logo image
        /// </summary>
        public string LogoUrl { get; set; } = string.Empty;

        /// <summary>
        /// URL to the favicon icon
        /// </summary>
        public string FaviconUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Color scheme and branding colors
    /// </summary>
    public class ColorSettings
    {
        /// <summary>
        /// Primary brand color in hex format (e.g., "#CC4141")
        /// </summary>
        public string Primary { get; set; } = "#CC4141";

        /// <summary>
        /// Secondary brand color in hex format (e.g., "#333333")
        /// </summary>
        public string Secondary { get; set; } = "#333333";

        /// <summary>
        /// Accent color for highlights in hex format (e.g., "#CC4141")
        /// </summary>
        public string Accent { get; set; } = "#CC4141";

        /// <summary>
        /// Default background color in hex format (e.g., "#FFFFFF")
        /// </summary>
        public string Background { get; set; } = "#FFFFFF";

        /// <summary>
        /// Default text color in hex format (e.g., "#000000")
        /// </summary>
        public string TextColor { get; set; } = "#000000";
    }

    /// <summary>
    /// Contact information and email configuration
    /// </summary>
    public class ContactSettings
    {
        /// <summary>
        /// Support email address for user inquiries (e.g., "support@ssw.com.au")
        /// </summary>
        public string SupportEmail { get; set; } = string.Empty;

        /// <summary>
        /// Marketing email address (e.g., "SSWMarketing@ssw.com.au")
        /// </summary>
        public string MarketingEmail { get; set; } = string.Empty;

        /// <summary>
        /// Staff email domain for identifying internal users (e.g., "ssw.com.au")
        /// </summary>
        public string StaffEmailDomain { get; set; } = string.Empty;

        /// <summary>
        /// Default sender email for system-generated emails (e.g., "verify@ssw.com.au")
        /// </summary>
        public string DefaultSenderEmail { get; set; } = string.Empty;

        /// <summary>
        /// Default sender name for system-generated emails (e.g., "SSW Rewards")
        /// </summary>
        public string DefaultSenderName { get; set; } = string.Empty;

        /// <summary>
        /// Email address to receive profile deletion requests (e.g., "SSWRewards@ssw.com.au")
        /// </summary>
        public string ProfileDeletionRecipient { get; set; } = string.Empty;
    }

    /// <summary>
    /// External service URLs and API endpoints
    /// </summary>
    public class ExternalServicesSettings
    {
        /// <summary>
        /// Base URL for the Rewards API (e.g., "https://api.rewards.ssw.com.au")
        /// </summary>
        public string ApiBaseUrl { get; set; } = string.Empty;

        /// <summary>
        /// Identity Server URL for authentication (e.g., "https://identity.ssw.com.au")
        /// </summary>
        public string IdentityServerUrl { get; set; } = string.Empty;

        /// <summary>
        /// Quiz service URL for quiz functionality (e.g., "https://quiz.ssw.com.au")
        /// </summary>
        public string QuizServiceUrl { get; set; } = string.Empty;

        /// <summary>
        /// Admin portal URL (e.g., "https://admin.rewards.ssw.com.au")
        /// </summary>
        public string AdminPortalUrl { get; set; } = string.Empty;
    }

    /// <summary>
    /// Social media URLs and profiles
    /// </summary>
    public class SocialMediaSettings
    {
        /// <summary>
        /// LinkedIn company page URL (e.g., "https://www.linkedin.com/company/ssw/")
        /// </summary>
        public string LinkedInUrl { get; set; } = string.Empty;

        /// <summary>
        /// Twitter/X profile URL (e.g., "https://twitter.com/SSW_TV")
        /// </summary>
        public string TwitterUrl { get; set; } = string.Empty;

        /// <summary>
        /// Facebook page URL (e.g., "https://www.facebook.com/SSW.page")
        /// </summary>
        public string FacebookUrl { get; set; } = string.Empty;

        /// <summary>
        /// Instagram profile URL (e.g., "https://www.instagram.com/ssw_tv/")
        /// </summary>
        public string InstagramUrl { get; set; } = string.Empty;

        /// <summary>
        /// YouTube channel URL (e.g., "https://www.youtube.com/user/sswtechtalks")
        /// </summary>
        public string YouTubeUrl { get; set; } = string.Empty;
    }
}
