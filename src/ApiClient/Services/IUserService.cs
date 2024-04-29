using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.ApiClient.Services;

public interface IUserService
{
    Task DeleteMyProfile(CancellationToken cancellationToken = default);

    Task<ProfilePicResponseDto> UploadProfilePic(Stream file, string fileName,
        CancellationToken cancellationToken = default);

    Task<CurrentUserDto> GetCurrentUser(CancellationToken cancellationToken = default);

    Task<UserAchievementsViewModel> GetProfileAchievements(int userId, CancellationToken cancellationToken = default);

    Task<UserProfileDto> GetUser(int userId, CancellationToken cancellationToken = default);

    Task<UserAchievementsViewModel> GetUserAchievements(int userId, CancellationToken cancellationToken = default);

    Task<UserRewardsViewModel> GetUserRewards(int userId, CancellationToken cancellationToken = default);

    Task<UserPendingRedemptionsViewModel> GetUserPendingRedemptions(int userId,
        CancellationToken cancellationToken = default);
}

public class UserService : IUserService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/User/";

    public UserService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task DeleteMyProfile(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}DeleteMyProfile", "", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to delete my profile: {responseContent}");
    }

    public async Task<ProfilePicResponseDto> UploadProfilePic(Stream file, string fileName,
        CancellationToken cancellationToken = default)
    {
        var content = Helpers.ProcessImageContent(file, fileName);

        var result = await _httpClient.PostAsync($"{_baseRoute}UploadProfilePic", content, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<ProfilePicResponseDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to upload proile pic: {responseContent}");
    }

    public async Task<CurrentUserDto> GetCurrentUser(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Get", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<CurrentUserDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get current user: {responseContent}");
    }

    public async Task<UserAchievementsViewModel> GetProfileAchievements(int userId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}ProfileAchievements?userId={userId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<UserAchievementsViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get profile achievements: {responseContent}");
    }

    public async Task<UserProfileDto> GetUser(int userId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetUser/{userId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<UserProfileDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get user: {responseContent}");
    }

    public async Task<UserAchievementsViewModel> GetUserAchievements(int userId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Achievements?userId={userId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<UserAchievementsViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get user achievements: {responseContent}");
    }

    public async Task<UserRewardsViewModel> GetUserRewards(int userId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}Rewards?userId={userId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<UserRewardsViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get user rewards: {responseContent}");
    }
    
    public async Task<UserPendingRedemptionsViewModel> GetUserPendingRedemptions(int userId, CancellationToken cancellationToken = default)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}GetUserPendingRedemptions?userId={userId}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<UserPendingRedemptionsViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get user pending redemptions: {responseContent}");
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