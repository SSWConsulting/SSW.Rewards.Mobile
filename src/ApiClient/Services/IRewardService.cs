using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.ApiClient.Services;

public interface IRewardService
{
    Task<RewardListViewModel> GetRewards(CancellationToken cancellationToken);
    Task<RewardListViewModel?> GetOnboardingRewards(CancellationToken cancellationToken);
    Task<RecentRewardListViewModel> GetRecentReward(DateTime? since, CancellationToken cancellationToken);
    Task<RewardListViewModel> SearchRewards(string searchTerm, CancellationToken cancellationToken);


    Task<ClaimRewardResult> RedeemReward(ClaimRewardDto claim, CancellationToken cancellationToken);

    Task<CreatePendingRedemptionResult> CreatePendingRedemption(ClaimRewardDto claim,
        CancellationToken cancellationToken);
}

public class RewardService : IRewardService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Reward/";

    public RewardService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<RewardListViewModel> GetRewards(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}List", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<RewardListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get rewards: {responseContent}");
    }

    public async Task<RewardListViewModel?> GetOnboardingRewards(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Onboarding", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<RewardListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get rewards: {responseContent}");
    }

    public async Task<RecentRewardListViewModel> GetRecentReward(DateTime? since, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetRecent?since={since}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<RecentRewardListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get rewards: {responseContent}");
    }

    public async Task<RewardListViewModel> SearchRewards(string searchTerm, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Search?searchTerm={searchTerm}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<RewardListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get rewards list: {responseContent}");
    }

    public async Task<ClaimRewardResult> RedeemReward(ClaimRewardDto claim, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}Claim", claim, cancellationToken);

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
    
    public async Task<CreatePendingRedemptionResult> CreatePendingRedemption(ClaimRewardDto claim, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}CreatePendingRedemption", claim, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<CreatePendingRedemptionResult>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to claim reward: {responseContent}");
    }
}
