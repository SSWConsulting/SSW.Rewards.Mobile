using SSW.Rewards.Helpers;

namespace SSW.Rewards
{
    public class Constants
    {
        private string authorityBase { get { return $"https://{AADDB2CTenantName}.b2clogin.com/tfp/{AADB2CTenantId}/"; } }

#if DEBUG
        // https://sswconsulting-dev.azurewebsites.net
        public string ApiBaseUrl = "https://baa1-203-220-43-154.ngrok.io";
        public string AppCenterAndroidId = "bfe53aa1-a7df-499d-900f-725a5222fc23";

#elif QA
        public string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
#else
        public string ApiBaseUrl = "https://sswconsulting-prod.azurewebsites.net";
        public string AppCenterAndroidId = "60b96e0a-c6dd-4320-855f-ed58e44ffd00";
#endif
        public string MaxApiSupportedVersion = "1.0";

        public string AADB2CClientId = Secrets.b2cClientId;

        public string IOSKeychainSecurityGroups = "com.companyname.SSW.Consulting";

        public string AADB2CPolicySignin = Secrets.b2cSigninPolicy;

        public string AADB2CTenantId = Secrets.b2cDomain;

        public string AADDB2CTenantName = Secrets.b2cTenantName;

        public string AADB2CPolicyReset = Secrets.b2cResetPolicy;

        public string[] Scopes => new string[] { $"https://{AADDB2CTenantName}.onmicrosoft.com/api/user_impersonation" };

        public string AuthoritySignin => $"{authorityBase}{AADB2CPolicySignin}";

        public string AuthorityReset => $"{authorityBase}{AADB2CPolicyReset}";

    }
}
