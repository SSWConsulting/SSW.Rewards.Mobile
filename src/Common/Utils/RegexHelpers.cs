using System.Text.RegularExpressions;

namespace SSW.Rewards.Shared.Utils;

public static partial class RegexHelpers
{
    private const string LinkedInValidationPattern = @"^https?://(www\.)?linkedin\.com/in/(?<username>[a-zA-Z0-9._-]+)$";
    private const string GitHubValidationPattern = @"^https?://(www\.)?github\.com/(?<username>[a-zA-Z0-9._-]+)$";
    private const string TwitterValidationPattern = @"^https?://(www\.)?(twitter|x)\.com/(?<username>[a-zA-Z0-9._-]+)$";
    private const string CompanyValidationPattern = @"^https?://\S+";

    [GeneratedRegex(@"^https?://", RegexOptions.IgnoreCase, 200)]
    public static partial Regex TitleRegex();

    [GeneratedRegex(LinkedInValidationPattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex LinkedInRegex();

    [GeneratedRegex(GitHubValidationPattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex GitHubRegex();

    [GeneratedRegex(TwitterValidationPattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex TwitterRegex();

    [GeneratedRegex(CompanyValidationPattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex CompanyRegex();

    // Extract username from social media URLs
    public static string ExtractUsername(this Regex regex, string url)
    {
        var match = regex.Match(url);
        return match.Success && match.Groups["username"].Success
            ? match.Groups["username"].Value
            : string.Empty;
    }
}