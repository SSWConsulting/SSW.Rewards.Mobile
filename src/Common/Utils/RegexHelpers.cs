using System.Text.RegularExpressions;

namespace SSW.Rewards.Shared.Utils;

public static partial class RegexHelpers
{
    private const string LinkedInValidationPattern = @"^https?://(www\.)?linkedin\.com/in/(?<handle>[a-zA-Z0-9._-]+)(?:\?.*)?$";
    private const string GitHubValidationPattern = @"^https?://(www\.)?github\.com/(?<handle>[a-zA-Z0-9._-]+)(?:\?.*)?$";
    private const string TwitterValidationPattern = @"^https?://(www\.)?(twitter|x)\.com/(?<handle>[a-zA-Z0-9._-]+)(?:\?.*)?$";
    private const string WebsitePattern = @"^https?://(?<handle>[a-zA-Z0-9._-]+\.[a-zA-Z]{2,})(?:/.*)?$";

    [GeneratedRegex(LinkedInValidationPattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex LinkedInRegex();

    [GeneratedRegex(GitHubValidationPattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex GitHubRegex();

    [GeneratedRegex(TwitterValidationPattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex TwitterRegex();

    [GeneratedRegex(WebsitePattern, RegexOptions.IgnoreCase, 200)]
    public static partial Regex WebsiteRegex();

    // Extract handle from URLs (username for social media, domain for company websites)
    public static string ExtractHandle(this Regex regex, string url)
    {
        var match = regex.Match(url);
        return match.Success && match.Groups["handle"].Success
            ? match.Groups["handle"].Value
            : string.Empty;
    }
}