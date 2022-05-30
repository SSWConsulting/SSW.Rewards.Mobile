using SSW.Rewards.Admin.Models.Achievements;
using System.Net.Http.Json;

namespace SSW.Rewards.Admin.Services;

public class AchievementsService
{
    private HttpClient _httpClient;
    public AchievementsService(IHttpClientFactory clientFactory)
    {
        this._httpClient = clientFactory.CreateClient("WebAPI");
    }

    public async Task<_AchievementAdminListViewModel?> GetAchievements()
    {
        return await this._httpClient.GetFromJsonAsync<_AchievementAdminListViewModel>("Achievement/AdminList");
    }
}
