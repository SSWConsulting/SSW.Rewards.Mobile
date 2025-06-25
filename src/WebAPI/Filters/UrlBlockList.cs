namespace SSW.Rewards.WebAPI.Filters;

public static class UrlBlockList
{
    public static readonly string[] SimpleContainsMatch =
    [
        ".git/config",
        "index/leaveMsg/",
        "kcfinder/",
        "wlwmanifest",
        "ALFA_DATA/",
        "wp-content",
    ];

    public static readonly string[] SimpleEndWithMatch =
    [
        ".php"
    ];

    public static bool IsBlocked(string url)
        => url switch
        {
            _ when string.IsNullOrWhiteSpace(url) => false,
            _ when SimpleContainsMatch.Any(x => url.Contains(x, StringComparison.OrdinalIgnoreCase)) => true,
            _ when SimpleEndWithMatch.Any(x => url.EndsWith(x, StringComparison.OrdinalIgnoreCase)) => true,
            _ => false
        };
}
