using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Consulting
{
    class Constants
    {
#if DEBUG
        public const string ApiBaseUrl = "https://b8a0da34.ngrok.io";//"https://sswconsulting-dev.azurewebsites.net";
#elif QA
        public const string ApiBaseUrl = "https://sswconsulting-dev.azurewebsites.net";
#else
        public const string ApiBaseUrl = "https://sswconsulting-prod.azurewebsites.net";
#endif
    }
}
