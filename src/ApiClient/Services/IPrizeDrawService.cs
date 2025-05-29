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
        var queryParams = new List<string>();
        
        if (filter.AchievementId.HasValue)
            queryParams.Add($"achievementId={filter.AchievementId}");
            
        if (filter.FilterStaff.HasValue)
            queryParams.Add($"filterStaff={filter.FilterStaff.Value}");
            
        if (filter.Top > 0)
            queryParams.Add($"top={filter.Top}");
            
        if (filter.DateFrom.HasValue)
            queryParams.Add($"dateFrom={filter.DateFrom.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");
            
        if (filter.DateTo.HasValue)
            queryParams.Add($"dateTo={filter.DateTo.Value:yyyy-MM-ddTHH:mm:ss.fffZ}");

        var queryString = queryParams.Count > 0 ? "?" + string.Join("&", queryParams) : "";
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