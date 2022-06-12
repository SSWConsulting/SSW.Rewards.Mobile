using SSW.Rewards.Api;

namespace SSW.Rewards.Admin.Services;

public class NotificationsService
{
    private readonly NotificationsClient _client;
    public NotificationsService(IHttpClientFactory clientFactory)
    {
        _client = new NotificationsClient(clientFactory.CreateClient(Constants.RewardsApiClient));
    }

    public async Task<NotificationHistoryListViewModel?> ListAsync()
    {
        return await this._client.ListAsync();
    }

    public async Task<Unit> AddAsync(RequestNotificationCommand command)
    {
        return await this._client.RequestPushAsync(command);
    }
}
