﻿namespace SSW.Rewards;

public static class Constants
{
#if DEBUG
    // public const string ApiBaseUrl = "https://app-sswrewards-api-staging.azurewebsites.net";
    // public const string ApiBaseUrl = "https://api.rewards.ssw.com.au";
    public const string ApiBaseUrl = "https://0a88-180-150-47-47.ngrok-free.app";
    public const string AppCenterAndroidId = "285df68b-ea1b-4afb-94c3-2581613c6880";
    public const string AppCenterIOSId = "71ea37dd-20c5-40ca-9d68-81b743d81337";

#elif QA
    public string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
#else
    public const string ApiBaseUrl = "https://api.rewards.ssw.com.au";
    public const string AppCenterAndroidId = "d6f591ec-8cef-44d7-96c0-08f31f91fb74";
    public const string AppCenterIOSId = "21efe682-dc49-4d39-8af8-ad05911be003";
#endif
    public const string MaxApiSupportedVersion = "1.0";

    public const string AuthRedirectUrl = "msauth.com.ssw.consulting://auth";
    public const string AutologinRedirectUrl = "sswrewards://autologin";

#if DEBUG
    public const string AuthorityUri = "https://identity.ssw.com.au";
    // public const string AuthorityUri = "https://app-ssw-ident-staging-api.azurewebsites.net";
#else
    public const string AuthorityUri = "https://identity.ssw.com.au";
#endif

    public const string ClientId = "ssw-rewards-mobile-app";

    public const string Scope = "openid profile ssw-rewards-api email offline_access";

    public const int AnimationRepeatCount = 3;

    public const string AUTHENTICATED_CLIENT = nameof(AUTHENTICATED_CLIENT);

    public static class SocialMediaPlatformIds
    {
        public const int GitHub = 1;
        public const int LinkedIn = 2;
        public const int Twitter = 3;
        public const int Company = 4;
    }
    
    public static class AnalyticsEvents
    {
        public const string QuizStart = "quiz_start";
        public const string QuizPass = "quiz_pass";
        public const string QuizFail = "quiz_fail";
        
        public const string RewardView = "reward_view";
        public const string RewardRedemptionPending = "reward_redemption_pending";
        public const string RewardRedemptionCancelled = "reward_redemption_cancelled";
        public const string RewardRedeemed = "reward_redeemed";
    }
}
