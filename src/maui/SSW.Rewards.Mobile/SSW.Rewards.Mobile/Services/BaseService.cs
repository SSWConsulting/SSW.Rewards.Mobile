using SSW.Rewards.Mobile.Helpers;

namespace SSW.Rewards.Services;

public class BaseService
{
    protected readonly HttpClient AuthenticatedClient;

    protected readonly string BaseUrl;

    public BaseService(IHttpClientFactory clientFactory, ApiOptions options)
    {
        AuthenticatedClient = clientFactory.CreateClient(AuthHandler.AuthenticatedClient);
        BaseUrl = options.BaseUrl;
    }
}
