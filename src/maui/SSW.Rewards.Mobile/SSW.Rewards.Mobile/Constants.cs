namespace SSW.Rewards;

public static class Constants
{
#if DEBUG
    public static string ApiBaseUrl = "https://9c0b-159-196-124-207.au.ngrok.io"; //"https://sswconsulting-dev.azurewebsites.net";
    public static string AppCenterAndroidId = "bfe53aa1-a7df-499d-900f-725a5222fc23";

#elif QA
    public string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
#else
    public static string ApiBaseUrl = "https://api.rewards.ssw.com.au";
    public static string AppCenterAndroidId = "60b96e0a-c6dd-4320-855f-ed58e44ffd00";
#endif
    public static string MaxApiSupportedVersion = "1.0";

    public static string AuthRedirectUrl { get; } = "msauth.com.ssw.consulting://auth";

#if DEBUG
    public static string AuthorityUri { get; } = "https://identity.ssw.com.au"; // "https://sswidentity-stage.azurewebsites.net";
#else
    public static string  AuthorityUri { get; } = "https://identity.ssw.com.au";
#endif
    public static string ClientId { get; } = "ssw-rewards-mobile-app";

    public static string Scope { get; } = "openid profile ssw-rewards-api email offline_access";
}
