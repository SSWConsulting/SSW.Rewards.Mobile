using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.ApiClient.Services;

public interface IUserAdminService
{
    Task<UsersViewModel> GetUsers(CancellationToken cancellationToken = default);
    Task<NewUsersViewModel> GetNewUsers(LeaderboardFilter filter, bool filterStaff, CancellationToken cancellationToken = default);
    Task UpdateUserRoles(UserDto dto, CancellationToken cancellationToken = default);
    Task<ProfileDeletionRequestsVieWModel> GetProfileDeletionRequests(CancellationToken cancellationToken = default);
    Task DeleteUserProfile(AdminDeleteProfileDto dto, CancellationToken cancellationToken = default);
}

public class UserAdminService : IUserAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/User/";

    public UserAdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }
    
    public async Task<UsersViewModel> GetUsers(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetUsers", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<UsersViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get users: {responseContent}");
    }
    
    public async Task UpdateUserRoles(UserDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}UpdateUserRoles", dto, cancellationToken);

        if  (!result.IsSuccessStatusCode)
        {
            var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

            throw new Exception($"Failed to update user roles: {responseContent}");
        }
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

    public async Task<ProfileDeletionRequestsVieWModel> GetProfileDeletionRequests(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetProfileDeletionRequests", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<ProfileDeletionRequestsVieWModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get profile deletion requests: {responseContent}");
    }

    public async Task DeleteUserProfile(AdminDeleteProfileDto dto, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}DeleteUserProfile", dto, cancellationToken);

        if  (!result.IsSuccessStatusCode)
        {
            var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

            throw new Exception($"Failed to delete user profile: {responseContent}");
        }
    }
}