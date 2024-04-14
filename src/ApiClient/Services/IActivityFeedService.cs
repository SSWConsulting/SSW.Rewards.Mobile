using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.ApiClient.Services;

public interface IActivityFeedService
{
    Task<ActivityFeedViewModel> GetAllActivities(int take, int skip, CancellationToken cancellationToken);
    Task<ActivityFeedViewModel> GetFriendsActivities(int take, int skip, CancellationToken cancellationToken);
}

public class ActivityFeedService(IHttpClientFactory clientFactory) : IActivityFeedService
{
    private readonly HttpClient _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);

    private const string _baseRoute = "api/ActivityFeed/";

    public async Task<ActivityFeedViewModel> GetAllActivities(int take, int skip, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetAllActivities?skip={skip}&take={take}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<ActivityFeedViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get all activities: {responseContent}");
    }
    
    public async Task<ActivityFeedViewModel> GetFriendsActivities(int take, int skip, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetFriendsActivities?skip={skip}&take={take}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<ActivityFeedViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get friends activities: {responseContent}");
    }
}