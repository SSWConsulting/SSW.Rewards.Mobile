using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.ApiClient.Services;

public interface ISkillsService
{
    Task<SkillsListViewModel> GetSkillsList(CancellationToken cancellationToken);
}

public class SkillsService : ISkillsService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Skills/";

    public SkillsService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<SkillsListViewModel> GetSkillsList(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<SkillsListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get skills list: {responseContent}");
    }
}