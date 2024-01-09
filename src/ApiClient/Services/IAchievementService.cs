using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.ApiClient.Services;

public interface IAchievementService
{
    Task<ClaimAchievementResult> ClaimAchievement(string code, CancellationToken cancellationToken);
    
    Task<AchievementListViewModel> GetAchievementList(CancellationToken cancellationToken);

    Task<AchievementUsersViewModel> GetAchievementUsers(int id, CancellationToken cancellationToken);

    Task<AchievementListViewModel> SearchAchievements(string searchTerm, CancellationToken cancellationToken);
}

public class AchievementService : IAchievementService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Achievement/";

    public AchievementService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<ClaimAchievementResult> ClaimAchievement(string code, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}Claim", new { code }, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<ClaimAchievementResult>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to claim achievement: {responseContent}");
    }

    public async Task<AchievementListViewModel> GetAchievementList(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<AchievementListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get achievement list: {responseContent}");
    }

    public async Task<AchievementUsersViewModel> GetAchievementUsers(int id, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Users?achievementId={id}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<AchievementUsersViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get achievement users: {responseContent}");
    }

    public async Task<AchievementListViewModel> SearchAchievements(string searchTerm, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Search?searchTerm={searchTerm}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<AchievementListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to search achievements: {responseContent}");
    }
}