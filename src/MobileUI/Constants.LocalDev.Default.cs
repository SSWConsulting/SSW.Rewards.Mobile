// Committed DEFAULTS for the mobile app's DEBUG API/identity URLs.
// Used when there is no git-ignored Constants.LocalDev.cs (fresh clone / CI).
// To switch locally, run:  dotnet run --project tools/RewardsDev -- api <target>
// (which writes Constants.LocalDev.cs and overrides these). Do not hand-edit URLs in Constants.cs.
namespace SSW.Rewards;

public static partial class Constants
{
    private const string LocalApiBaseUrl = "https://app-sswrewards-api-staging.azurewebsites.net";
    private const string LocalAuthorityUri = "https://identity.ssw.com.au";
}
