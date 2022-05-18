using SSW.Rewards.Helpers;

namespace SSW.Rewards
{
    public class Constants
    {
        private string authorityBase { get { return $"https://{AADDB2CTenantName}.b2clogin.com/tfp/{AADB2CTenantId}/"; } }

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

        public string AADB2CClientId = "bb80971c-3a85-4d6d-aef4-cf0baf0f374b";

        public string IOSKeychainSecurityGroups = "com.companyname.SSW.Consulting";

        public string AADB2CPolicySignin = "B2C_1A_Signup_Signin";

        public string AADB2CTenantId = "sswconsultingapp.onmicrosoft.com";

        public string AADDB2CTenantName = "sswconsultingapp";

        public string AADB2CPolicyReset = "B2C_1_PasswordReset";

        public string[] Scopes => new string[] { $"https://{AADDB2CTenantName}.onmicrosoft.com/api/user_impersonation" };

        public string AuthoritySignin => $"{authorityBase}{AADB2CPolicySignin}";

        public string AuthorityReset => $"{authorityBase}{AADB2CPolicyReset}";

    }
}
