using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.ApiClient.Services;

public interface IUserAdminService
{
    Task<NewUsersViewModel> GetNewUsers(LeaderboardFilter filter, bool filterStaff, CancellationToken cancellationToken = default);
}

public class UserAdminService : IUserAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/User/";

    public UserAdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }
    
    public async Task<NewUsersViewModel> GetNewUsers(LeaderboardFilter filter, bool filterStaff, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetNewUsers?filter={filter}&filterStaff={filterStaff}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<NewUsersViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get new users: {responseContent}");
    }
}