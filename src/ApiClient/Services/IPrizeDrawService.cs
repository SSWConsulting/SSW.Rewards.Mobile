using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.PrizeDraw;

namespace SSW.Rewards.ApiClient.Services;

public interface IPrizeDrawService
{
    Task<EligibleUsersViewModel> GetEligibleUsers(GetEligibleUsersFilter filter, CancellationToken cancellationToken);
}

public class PrizeDrawService : IPrizeDrawService
{
    private readonly HttpClient _httpClient;

    // TODO: implement this in a separate controller and change the leaderboard query back to just filtering by time
    private const string _baseRoute = "api/Leaderboard/";

    public PrizeDrawService(IHttpClientFactory clientFactory)
    {
        _httpClient = clientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<EligibleUsersViewModel> GetEligibleUsers(GetEligibleUsersFilter filter, CancellationToken cancellationToken)
    {
        var queryString = $"?achievementId={filter.AchievementId}" +
                          $"&filterStaff={filter.FilterStaff}" +
                          $"&top={filter.Top}" +
                          $"&dateFrom={filter.DateFrom:yyyy-MM-ddTHH:mm:ss.fffZ}" +
                          $"&dateTo={filter.DateTo:yyyy-MM-ddTHH:mm:ss.fffZ}";

        var result = await _httpClient.GetAsync($"{_baseRoute}GetEligibleUsers{queryString}", cancellationToken);

        if (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<EligibleUsersViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to get eligible users: {responseContent}");
    }
}