using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Leaderboard;


namespace SSW.Rewards.ApiClient.Services;

public interface IActivityFeedService
{
    Task<IEnumerable<ActivityFeedViewModel>> GetAllActivities(CancellationToken cancellationToken);
    Task<IEnumerable<ActivityFeedViewModel>> GetFriendsActivities(CancellationToken cancellationToken);
}

public class ActivityFeedService : IActivityFeedService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/ActivityFeed/";

    public ActivityFeedService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<IEnumerable<ActivityFeedViewModel>> GetAllActivities(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetAllActivities", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IEnumerable<ActivityFeedViewModel>>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get all activities: {responseContent}");
    }
    
    public async Task<IEnumerable<ActivityFeedViewModel>> GetFriendsActivities(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetFriendsActivities", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IEnumerable<ActivityFeedViewModel>>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get friends activities: {responseContent}");
    }
}