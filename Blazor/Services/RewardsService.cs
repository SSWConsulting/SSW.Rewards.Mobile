using SSW.Rewards.Admin.Models.Achievements;
using SSW.Rewards.Admin.Models.Rewards;
using System.Net.Http.Json;

namespace SSW.Rewards.Admin.Services;

public class RewardsService
{
    private HttpClient _httpClient;
    public RewardsService(IHttpClientFactory clientFactory)
    {
        this._httpClient = clientFactory.CreateClient("WebAPI");
    }

    public async Task<_RewardAdminListViewModel?> GetRewards()
    {
        return await this._httpClient.GetFromJsonAsync<_RewardAdminListViewModel>("Reward/AdminList");
    }
}
