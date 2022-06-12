using SSW.Rewards.Admin.Models.Leaderboard;
using SSW.Rewards.Api;
using System.Net.Http.Json;

namespace SSW.Rewards.Admin.Services;

public class LeaderboardService
{
    private readonly LeaderboardClient _client;
    public LeaderboardService(IHttpClientFactory clientFactory)
    {
        _client = new LeaderboardClient(clientFactory.CreateClient(Constants.RewardsApiClient));
    }

    public async Task<LeaderboardListViewModel?> GetAsync()
    {
        return await _client.GetAsync();
    }
}
