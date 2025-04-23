using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Leaderboard;


namespace SSW.Rewards.ApiClient.Services;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken, int take = 0, int skip = 0);
    Task<LeaderboardViewModel> GetLeaderboard(LeaderboardFilter filter, CancellationToken cancellationToken);
}

public class LeaderboardService : ILeaderboardService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Leaderboard/";

    public LeaderboardService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken, int take = 0, int skip = 0)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Get?skip={skip}&take={take}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<LeaderboardViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get leaderboard: {responseContent}");
    }

    public async Task<LeaderboardViewModel> GetLeaderboard(LeaderboardFilter filter, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetEligibleUsers?filter={filter}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<LeaderboardViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get leaderboard: {responseContent}");
    }
}