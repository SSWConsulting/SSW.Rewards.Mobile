using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.ApiClient.Services;

public interface IAchievementAdminService
{
    Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken);

    Task UpdateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken);

    Task DeleteAchievement(int id, CancellationToken cancellationToken);

    Task<AchievementAdminListViewModel> GetAdminAchievementList(CancellationToken cancellationToken);

    Task<ClaimAchievementResult> ClaimAchievementForUser(string code, int userId, CancellationToken cancellationToken);
}

public class AchievementAdminService : IAchievementAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Achievement/";

    public AchievementAdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<ClaimAchievementResult> ClaimAchievementForUser(string code, int userId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}ClaimForUser", new { code, userId }, cancellationToken);

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

    public async Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}", achievement, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<AchievementAdminDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to create achievement: {responseContent}");
    }

    public async Task DeleteAchievement(int id, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}?id={id}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to delete achievement: {responseContent}");
    }

    public async Task<AchievementAdminListViewModel> GetAdminAchievementList(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetFromJsonAsync<AchievementAdminListViewModel>($"{_baseRoute}AdminList", cancellationToken);

        if (result is not null)
        {
            return result;
        }

        throw new Exception("Failed to get achievement list");
    }

    public async Task UpdateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PatchAsJsonAsync($"{_baseRoute}", achievement, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to update achievement: {responseContent}");
    }
}
