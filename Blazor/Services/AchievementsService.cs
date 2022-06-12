using SSW.Rewards.Admin.Models.Achievements;
using SSW.Rewards.Api;
using System.Net.Http.Json;

namespace SSW.Rewards.Admin.Services;

public class AchievementsService
{
    private readonly AchievementClient _client;
    public AchievementsService(IHttpClientFactory clientFactory)
    {
        _client = new AchievementClient(clientFactory.CreateClient(Constants.RewardsApiClient));
    }

    public async Task<AchievementAdminListViewModel?> AdminListAsync()
    {
        return await _client.AdminListAsync();
    }

    public async Task CreateAsync(CreateAchievementCommand command)
    {
        await _client.CreateAsync(command);
    }

    public async Task DeleteAsync(DeleteAchievementCommand command)
    {
        await _client.DeleteAsync(command);
    }
}
