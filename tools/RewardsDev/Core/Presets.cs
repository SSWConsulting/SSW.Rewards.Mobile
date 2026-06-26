namespace SSW.Rewards.DevTool.Core;

// The known API / identity targets and the safe committed defaults.
public static class Presets
{
    public const string DefaultApi = "https://app-sswrewards-api-staging.azurewebsites.net";
    public const string DefaultAuthority = "https://identity.ssw.com.au";
    public const int ApiPort = 5001; // used for the tailscale preset

    public static readonly IReadOnlyDictionary<string, string> Api = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["local"]   = "https://localhost:5001",
        ["staging"] = "https://app-sswrewards-api-staging.azurewebsites.net",
        ["prod"]    = "https://api.rewards.ssw.com.au",
    };

    public static readonly IReadOnlyDictionary<string, string> Identity = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["local"]   = "https://localhost:14330",
        ["staging"] = "https://app-ssw-ident-staging-api.azurewebsites.net",
        ["prod"]    = "https://identity.ssw.com.au",
    };
}
