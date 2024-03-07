using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Network;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.ApiClient.Services;

public interface IStaffService
{
    Task<StaffListViewModel> GetStaffList(CancellationToken cancellationToken);

    Task<StaffMemberDto> GetStaffMember(int id, CancellationToken cancellationToken);

    Task<StaffMemberDto> SearchStaffMember(StaffMemberQueryDto query, CancellationToken cancellationToken);
    
    Task<NetworkProfileListViewModel> GetNetworkProfileList(CancellationToken cancellationToken);
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
        var result = await _httpClient.GetAsync($"{_baseRoute}Get", cancellationToken);

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
        var result = await _httpClient.GetAsync($"{_baseRoute}GetStaffMemberProfile?id={id}", cancellationToken);

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
        var result = await _httpClient.GetAsync($"{_baseRoute}GetStaffMemberByEmail?email={query.email}", cancellationToken);

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
    
    public async Task<NetworkProfileListViewModel> GetNetworkProfileList(CancellationToken cancellationToken)
    {
        var result = await _httpClient.GetAsync("api/Network/GetList", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<NetworkProfileListViewModel>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);

        throw new Exception($"Failed to get network profile list: {responseContent}");
    }
}