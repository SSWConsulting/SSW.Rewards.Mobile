using Newtonsoft.Json.Linq;


namespace SSW.Rewards.Helpers;

public class ApiInfo
{
    private string _baseUrl { get; set; }
    private HttpClient _client;

    public ApiInfo(string baseUrl)
    {
        _baseUrl = baseUrl;
        _client = new HttpClient();
    }

    public async Task<string> GetApiVersionAsync()
    {
        Uri requestUri = new Uri(_baseUrl + "/swagger/SSW.Rewards%20API/swagger.json");
        var apiInfo = await _client.GetAsync(requestUri);

        if(apiInfo.IsSuccessStatusCode)
        {
            var content = await apiInfo.Content.ReadAsStringAsync();
            dynamic o = JObject.Parse(content);

            return o.info.version;
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
        {
            return true;
        }

        float apiVerf = float.Parse(apiVer);
        float appVerf = float.Parse(appVer);

        if (apiVerf > appVerf)
        {
            return false;
        }
        else
        {
            return true;
        }
    }
}
