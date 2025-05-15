using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Leaderboard;


namespace SSW.Rewards.ApiClient.Services;

public interface ILeaderboardService
{
    Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken);
    Task<LeaderboardViewModel> GetPaginatedLeaderboard(int take, int skip, LeaderboardFilter currentPeriod, CancellationToken cancellationToken);
    Task<LeaderboardViewModel> GetLeaderboard(LeaderboardFilter filter, CancellationToken cancellationToken);
    Task<MobileLeaderboardViewModel> GetMobilePaginatedLeaderboard(int page, int pageSize, LeaderboardFilter currentPeriod, CancellationToken cancellationToken);
}

public class LeaderboardService : ILeaderboardService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Leaderboard/";

    public LeaderboardService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<LeaderboardViewModel> GetLeaderboard(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Get", cancellationToken);

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

    public async Task<MobileLeaderboardViewModel> GetMobilePaginatedLeaderboard(int page, int pageSize, LeaderboardFilter currentPeriod, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetMobilePaginated?page={page}&pageSize={pageSize}&currentPeriod={currentPeriod}", cancellationToken);
        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<MobileLeaderboardViewModel>(cancellationToken: cancellationToken);
            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get leaderboard: {responseContent}");
    }

    public async Task<LeaderboardViewModel> GetPaginatedLeaderboard(int take, int skip, LeaderboardFilter currentPeriod, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetPaginated?skip={skip}&take={take}&currentPeriod={currentPeriod}", cancellationToken);

        if (result.IsSuccessStatusCode)
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