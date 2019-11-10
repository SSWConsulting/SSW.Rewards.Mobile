using System;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace SSW.Consulting.Helpers
{
    public class ApiInfo
    {
        private string _baseUrl { get; set; }
        private HttpClient _client;

        public ApiInfo(string BaseURL)
        {
            _baseUrl = BaseURL;
            _client = new HttpClient();
        }

        public async Task<string> GetApiVersionAsync()
        {
            string requestUri = _baseUrl + "/swagger/SSW.Consulting%20API/swagger.json";
            var apiInfo = await _client.GetAsync(requestUri);

            if(apiInfo.IsSuccessStatusCode)
            {
                var content = await apiInfo.Content.ReadAsStringAsync();
                dynamic o = JObject.Parse(content);

                string ver = o.info.version;

                return ver;
            }
            else
            {
                return "failed";
            }
        }

        public async Task<bool> IsApiCompatibleAsync()
        {
            string appVer = Constants.MaxApiSupportedVersion;

            string apiVer = await GetApiVersionAsync();

            if (apiVer == appVer)
                return true;

            float apiVerf = float.Parse(apiVer);
            float appVerf = float.Parse(appVer);

            if (apiVerf > appVerf)
                return false;
            else
                return true;
        }
    }
}
