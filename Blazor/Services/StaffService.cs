using SSW.Rewards.Admin.Models.Staff;
using System.Net.Http.Json;

namespace SSW.Rewards.Admin.Services;

public class StaffService
{
    private HttpClient _httpClient;
    public StaffService(IHttpClientFactory clientFactory)
    {
        this._httpClient = clientFactory.CreateClient(Constants.RewardsApiClient);
    }

    public async Task<_StaffListViewModel?> GetStaff()
    {
        return await this._httpClient.GetFromJsonAsync<_StaffListViewModel>("Staff/Get");
    }
}
