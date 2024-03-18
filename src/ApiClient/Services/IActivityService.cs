using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.ApiClient.Services;

public interface IActivityService
{
    Task<IList<ActivityFeedViewModel>> GetActivityFeed(ActivityFeedFilter filter, int skip, int take, CancellationToken cancellationToken);
}

public class ActivityService : IActivityService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/ActivityFeed/";

    public ActivityService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<IList<ActivityFeedViewModel>> GetActivityFeed(ActivityFeedFilter filter, int skip, int take, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetActivities?filter={filter}&skip={skip}&take={take}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IList<ActivityFeedViewModel>>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to fetch Activity Feed: {responseContent}");
    }
}
