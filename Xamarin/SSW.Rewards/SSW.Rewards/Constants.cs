namespace SSW.Rewards
{
    public class Constants
    {
#if DEBUG
        public string ApiBaseUrl = "https://sswconsulting-prod.azurewebsites.net";//"https://147c-159-196-124-207.ngrok.io";
        public string AppCenterAndroidId = "bfe53aa1-a7df-499d-900f-725a5222fc23";

#elif QA
        public string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
#else
        public string ApiBaseUrl = "https://sswconsulting-prod.azurewebsites.net";
        public string AppCenterAndroidId = "60b96e0a-c6dd-4320-855f-ed58e44ffd00";
#endif
        public string MaxApiSupportedVersion = "1.0";

        public string AuthRedirectUrl { get; } = "auth.com.ssw.rewards://callback";

        public string AccessToken { get; set; }

        public string AuthorityUri { get; } = "https://sswidentity-stage.azurewebsites.net/";

        public string ClientId { get; } = "ssw-rewards-mobile-app";

        public string Scope { get; } = "openid profile offline_access ssw-rewards-api";

    }
}
