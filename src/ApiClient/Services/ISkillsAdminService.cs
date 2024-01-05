using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.ApiClient.Services;

public interface ISkillsAdminService
{
    Task DeleteSkill(int skillId, CancellationToken cancellationToken);

    Task<int> AddOrUpdateSkill(SkillDto skill, CancellationToken cancellationToken);
}

public class SkillsAdminService : ISkillsAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Skills/";

    public SkillsAdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DeleteSkill(int skillId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}{skillId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to delete skill: {responseContent}");
    }

    public async Task<int> AddOrUpdateSkill(SkillDto skill, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}", skill, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to add or update skill: {responseContent}");
    }
}
