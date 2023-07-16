namespace SSW.Rewards
{
    public class Constants
    {
#if DEBUG
        public string ApiBaseUrl = "https://api.rewards.ssw.com.au";
        public string AppCenterAndroidId = "60b96e0a-c6dd-4320-855f-ed58e44ffd00";

#elif QA
        public string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
#else
        public string ApiBaseUrl = "https://api.rewards.ssw.com.au";
        public string AppCenterAndroidId = "60b96e0a-c6dd-4320-855f-ed58e44ffd00";
#endif
        public string MaxApiSupportedVersion = "1.0";

        public string AuthRedirectUrl { get; } = "msauth.com.ssw.consulting://auth";

#if DEBUG
        public string AuthorityUri { get; } = "https://identity.ssw.com.au";
#else
        public string AuthorityUri { get; } = "https://identity.ssw.com.au";
#endif
        public string ClientId { get; } = "ssw-rewards-mobile-app";

        public string Scope { get; } = "openid profile ssw-rewards-api email offline_access";

        public const string PointsAwardedMessage = "PointsAwarded";

        public const string EnableScannerMessage = "EnableScanner";

    }
}
