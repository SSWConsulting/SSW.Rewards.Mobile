using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.ApiClient.Services;

public interface IRewardAdminService
{
    Task<RewardsAdminViewModel> GetRewards(CancellationToken cancellationToken);
    Task<int> AddReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task UpdateReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task DeleteReward(int rewardId, CancellationToken cancellationToken);

    Task<ClaimRewardResult> ClaimForUser(string code, int userId, bool isPendingRedemption, CancellationToken cancellationToken);
}

public class RewardAdminService : IRewardAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Reward/";

    public RewardAdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<RewardsAdminViewModel> GetRewards(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}AdminList", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<RewardsAdminViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get rewards: {result}");
    }

    public async Task<int> AddReward(RewardEditDto reward, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}Add", reward, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to add reward: {responseContent}");
    }

    public async Task UpdateReward(RewardEditDto reward, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PatchAsJsonAsync($"{_baseRoute}UpdateReward", reward, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to update reward: {responseContent}");
    }

    public async Task DeleteReward(int rewardId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}Delete/    {rewardId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to delete reward: {responseContent}");
    }

    public async Task<ClaimRewardResult> ClaimForUser(string code, int userId, bool isPendingRedemption, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}ClaimForUser", new { code, userId, isPendingRedemption }, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<ClaimRewardResult>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to claim reward: {responseContent}");
    }
}