namespace SSW.Rewards.ApiClient;

public class Constants
{
    public const string AuthenticatedClient = "AuthenticatedClient";

    public const string RewardsAppClientNameHeaderKey = "RewardsApp-ClientName";
    public const string RewardsAppClientName_AdminUI = "AdminUI";
    public const string RewardsAppClientName_MobileApp = "MobileApp";

    // QR Code Constants (e.g. sswrewards://redeem?code={{ach:123456 in base64}})
    public const string RewardsQRCodeProtocol = "sswrewards://";
    public const string RewardsQRCodeProtocolQueryName = "code";

    public const string RewardsQRCodeAchievementPrefix = "ach:";
    public const string RewardsQRCodeRewardPrefix = "rwd:";
    public const string RewardsQRCodePendingRewardPrefix = "pnd:";

    public static readonly string[] SupportedQRCodeBodyPrefixes =
    [
        RewardsQRCodeAchievementPrefix,
        RewardsQRCodeRewardPrefix,
        RewardsQRCodePendingRewardPrefix
    ];

    public static readonly string RewardsQRCodeUrlFormat = $"{RewardsQRCodeProtocol}://redeem?{RewardsQRCodeProtocolQueryName}={{0}}";
}
