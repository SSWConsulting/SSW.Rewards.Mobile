using SSW.Rewards.Mobile.Helpers;

namespace SSW.Rewards.Services;

public class BaseService
{
    protected static HttpClient AuthenticatedClient;

    protected readonly string BaseUrl;

    public BaseService(IHttpClientFactory clientFactory, ApiOptions options)
    {
        if (AuthenticatedClient is null) AuthenticatedClient = clientFactory.CreateClient(AuthHandler.AuthenticatedClient);
        BaseUrl = options.BaseUrl;
    }
}
