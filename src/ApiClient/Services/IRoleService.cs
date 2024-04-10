using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Roles;

namespace SSW.Rewards.ApiClient.Services;

public interface IRoleService
{
    Task<IEnumerable<RoleDto>> GetRoles(CancellationToken cancellationToken = default);
}

public class RoleService : IRoleService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Role/";

    public RoleService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<IEnumerable<RoleDto>> GetRoles(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetRoles", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<IEnumerable<RoleDto>>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get roles: {responseContent}");
    }
}