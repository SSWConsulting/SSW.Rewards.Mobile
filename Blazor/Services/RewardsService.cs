using SSW.Rewards.Admin.Models.Achievements;
using SSW.Rewards.Admin.Models.Rewards;
using SSW.Rewards.Api;
using System.Net.Http.Json;

namespace SSW.Rewards.Admin.Services;

public class RewardsService
{
    private readonly RewardClient _client;
    public RewardsService(IHttpClientFactory clientFactory)
    {
        _client = new RewardClient(clientFactory.CreateClient(Constants.RewardsApiClient));
    }

    public async Task<RewardAdminListViewModel?> AdminListAsync()
    {
        return await _client.AdminListAsync();
    }

    public async Task DeleteAsync(DeleteRewardCommand command)
    {
        await _client.DeleteAsync(command);
    }

    public async Task AddAsync(AddRewardCommand command)
    {
        await _client.AddAsync(command);
    }
}
