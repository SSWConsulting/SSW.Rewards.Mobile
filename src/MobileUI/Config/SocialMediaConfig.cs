using SSW.Rewards.Shared.Utils;

namespace SSW.Rewards.Mobile.Config;

public class SocialMediaConfig
{
    public Dictionary<int, SocialMediaPlatform> Platforms { get; } = new()
    {
        [Constants.SocialMediaPlatformIds.LinkedIn] = new SocialMediaPlatform
        {
            PlatformName = "LinkedIn",
            Url = "https://linkedin.com/in/",
            Placeholder = "[your-name]",
            ValidationPattern = RegexHelpers.LinkedInRegex,
            Icon = "\\uf0e1"
        },
        [Constants.SocialMediaPlatformIds.GitHub] = new SocialMediaPlatform
        {
            PlatformName = "GitHub",
            Url = "https://github.com/",
            Placeholder = "[your-username]",
            ValidationPattern = RegexHelpers.GitHubRegex,
            Icon = "\\uf09b"
        },
        [Constants.SocialMediaPlatformIds.Twitter] = new SocialMediaPlatform
        {
            PlatformName = "Twitter",
            Url = "https://x.com/",
            Placeholder = "[your-username]",
            ValidationPattern = RegexHelpers.TwitterRegex,
            Icon = "\\ue61b"
        },
        [Constants.SocialMediaPlatformIds.Company] = new SocialMediaPlatform
        {
            PlatformName = "Company",
            Url = "https://",
            Placeholder = "[your-website]",
            ValidationPattern = RegexHelpers.WebsiteRegex,
            Icon = "\\uf1ad"
        }
    };
}