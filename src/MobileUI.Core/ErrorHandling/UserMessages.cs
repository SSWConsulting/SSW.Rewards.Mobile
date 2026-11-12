namespace SSW.Rewards.Mobile.Common;

/// <summary>
/// The one place for user-facing error wording, so every page says "offline" the same way.
/// Pass a lower-case subject ("the activity feed", "rewards") or action ("redeem a reward").
/// </summary>
public static class UserMessages
{
    public const string OfflineTitle = "You're offline";
    public const string ErrorTitle = "Something went wrong";

    /// <summary>Shown on the scan result popup when a code can't be claimed offline.</summary>
    public const string ScanOffline = "Connect to the internet and scan again.";

    public static string OfflineLoad(string subject) => $"Connect to the internet to load {subject}.";

    public static string ErrorLoad(string subject) => $"We couldn't load {subject}. Please try again soon.";

    public static string OfflineAction(string action) => $"Connect to the internet to {action}.";

    public static string ErrorAction(string action) => $"We couldn't {action}. Please try again soon.";
}
