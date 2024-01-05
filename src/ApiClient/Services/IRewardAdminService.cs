using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.ApiClient.Services;

public interface IRewardAdminService
{
    Task<RewardsAdminViewModel> GetRewards(CancellationToken cancellationToken);
    Task<int> AddReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task UpdateReward(RewardEditDto reward, CancellationToken cancellationToken);
    Task DeleteReward(int rewardId, CancellationToken cancellationToken);

    Task<ClaimRewardResult> ClaimForUser(string code, int userId, CancellationToken cancellationToken);
}

public class RewardAdminService : IRewardAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Reward/";

    public RewardAdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<RewardsAdminViewModel> GetRewards(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<RewardsAdminViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get rewards: {responseContent}");
    }

    public async Task<int> AddReward(RewardEditDto reward, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}", reward, cancellationToken);

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
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}", reward, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to update reward: {responseContent}");
    }

    public async Task DeleteReward(int rewardId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}{rewardId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to delete reward: {responseContent}");
    }

    public async Task<ClaimRewardResult> ClaimForUser(string code, int userId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}ClaimForUser", new { code, userId }, cancellationToken);

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