using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards
{
    class Constants
    {
#if DEBUG
        public const string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
        public const string AppCenterAndroidId = "bfe53aa1-a7df-499d-900f-725a5222fc23";

#elif QA
        public const string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
#else
        public const string ApiBaseUrl = "https://sswconsulting-prod.azurewebsites.net";
        public const string AppCenterAndroidId = "60b96e0a-c6dd-4320-855f-ed58e44ffd00";
#endif
        public const string MaxApiSupportedVersion = "1.0";
    }
}
