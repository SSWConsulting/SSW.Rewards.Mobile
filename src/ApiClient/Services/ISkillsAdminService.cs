﻿using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.ApiClient.Services;

public interface ISkillsAdminService
{
    Task DeleteSkill(int skillId, CancellationToken cancellationToken);

    Task<int> AddOrUpdateSkill(SkillEditDto skill, CancellationToken cancellationToken);
}

public class SkillsAdminService : ISkillsAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Skill/";

    public SkillsAdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task DeleteSkill(int skillId, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}DeleteSkill?id={skillId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to delete skill: {responseContent}");
    }

    public async Task<int> AddOrUpdateSkill(SkillEditDto skill, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}UpsertSkill", skill, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<int>(cancellationToken: cancellationToken);

            return response;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to add or update skill: {responseContent}");
    }
}
