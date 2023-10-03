using SSW.Rewards.Mobile.Helpers;

namespace SSW.Rewards.Mobile.Services;

public class ApiBaseService
{
    protected static HttpClient AuthenticatedClient;

    protected readonly string BaseUrl;

    public ApiBaseService(IHttpClientFactory clientFactory, ApiOptions options)
    {
        if (AuthenticatedClient is null) AuthenticatedClient = clientFactory.CreateClient(AuthHandler.AuthenticatedClient);
        BaseUrl = options.BaseUrl;
    }
}
