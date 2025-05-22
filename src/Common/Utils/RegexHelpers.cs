using System.Text.RegularExpressions;

namespace SSW.Rewards.Shared.Utils;

public static class RegexHelpers
{
    private static readonly Regex _titleRegex = new(@"^https?://", RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
    private static readonly Regex _linkedInRegex = new(LinkedInValidationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
    private static readonly Regex _gitHubRegex = new(GitHubValidationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
    private static readonly Regex _twitterRegex = new(TwitterValidationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));
    private static readonly Regex _companyRegex = new(CompanyValidationPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(200));

    public const string LinkedInValidationPattern = "^https?://(www.)?linkedin.com/in/([a-zA-Z0-9._-]+)$";
    public const string GitHubValidationPattern = "^https?://(www.)?github.com/([a-zA-Z0-9._-]+)$";
    public const string TwitterValidationPattern = "^https?://(www.)?(twitter|x).com/([a-zA-Z0-9._-]+)$";
    public const string CompanyValidationPattern = @"^https?://\S+";

    /// <summary>
    /// Use old school compile Regex. In MAUI .NET 8 generated Regex results in infinite compile.
    /// When upgraded to .NET 9, try
    /// [GeneratedRegex(@"^https?://")]
    /// public static Regex HttpRegex ParseTitle();
    /// </summary>
    public static Regex TitleRegex() => _titleRegex;

    public static Regex SocialRegexByPattern(string validationPattern)
        => validationPattern switch
        {
            LinkedInValidationPattern => _linkedInRegex,
            GitHubValidationPattern => _gitHubRegex,
            TwitterValidationPattern => _twitterRegex,
            CompanyValidationPattern => _companyRegex,
            _ => new Regex(validationPattern, RegexOptions.IgnoreCase)
        };
}
