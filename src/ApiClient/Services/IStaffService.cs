using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.ApiClient.Services;

public interface IStaffService
{
    Task<StaffListViewModel> GetStaffList(CancellationToken cancellationToken);

    Task<StaffMemberDto> GetStaffMember(int id, CancellationToken cancellationToken);

    Task<StaffMemberDto> SearchStaffMember(StaffMemberQueryDto query, CancellationToken cancellationToken);
}

public class StaffService : IStaffService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Staff/";

    public StaffService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public async Task<StaffListViewModel> GetStaffList(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<StaffListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get staff list: {responseContent}");
    }

    public async Task<StaffMemberDto> GetStaffMember(int id, CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync($"{_baseRoute}{id}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<StaffMemberDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get staff member: {responseContent}");
    }

    public async Task<StaffMemberDto> SearchStaffMember(StaffMemberQueryDto query, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PostAsJsonAsync($"{_baseRoute}Search", query, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<StaffMemberDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to search staff member: {responseContent}");
    }
}