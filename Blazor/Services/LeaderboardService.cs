using SSW.Rewards.Admin.Models.Leaderboard;
using System.Net.Http.Json;

namespace SSW.Rewards.Admin.Services;

public class LeaderboardService
{
    private HttpClient _httpClient;
    public LeaderboardService(IHttpClientFactory clientFactory)
    {
        this._httpClient = clientFactory.CreateClient(Constants.RewardsApiClient);
    }

    public async Task<_LeaderboardListViewModel?> GetLeaderboard()
    {
        return await this._httpClient.GetFromJsonAsync<_LeaderboardListViewModel>("Leaderboard/Get");
    }
}
