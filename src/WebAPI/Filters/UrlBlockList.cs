using System.Buffers;

namespace SSW.Rewards.WebAPI.Filters;

public static class UrlBlockList
{
    private static string[] _simpleContainsMatch = Array.Empty<string>();
    private static SearchValues<string> _simpleContainsMatchSearchValues = SearchValues.Create(_simpleContainsMatch, StringComparison.OrdinalIgnoreCase);

    public static void Init(string[]? simpleContainsMatch)
    {
        _simpleContainsMatch = simpleContainsMatch ?? [];
        _simpleContainsMatch = _simpleContainsMatch
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim().ToLowerInvariant())
            .ToArray();

        _simpleContainsMatchSearchValues = SearchValues.Create(_simpleContainsMatch, StringComparison.Ordinal);
    }

    public static bool IsBlocked(string url)
        => url switch
        {
            _ when string.IsNullOrWhiteSpace(url) => false,
            _ when url.ToLowerInvariant().AsSpan().ContainsAny(_simpleContainsMatchSearchValues) => true,
            _ => false
        };
}
