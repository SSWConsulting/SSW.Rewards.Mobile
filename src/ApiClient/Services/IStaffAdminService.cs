using System.Net.Http.Json;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.ApiClient.Services;

public interface IStaffAdminService
{
    Task DeleteStaffMember(int id, CancellationToken cancellationToken);

    Task UploadProfilePicture(int id, Stream file, CancellationToken cancellationToken);

    Task<StaffMemberDto> UpsertStaffMemberProfile(StaffMemberDto dto, CancellationToken cancellationToken);
}

public class StaffAdminService : IStaffAdminService
{
    private readonly HttpClient _httpClient;

    private const string _baseRoute = "api/Staff/";

    public StaffAdminService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task DeleteStaffMember(int id, CancellationToken cancellationToken)
    {
        var result = await _httpClient.DeleteAsync($"{_baseRoute}{id}", cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to delete staff member: {responseContent}");
    }

    public async Task UploadProfilePicture(int id, Stream file, CancellationToken cancellationToken)
    {
        var content = new MultipartFormDataContent();
        content.Add(new StreamContent(file), "file", "file");

        var result = await _httpClient.PostAsync($"{_baseRoute}{id}/UploadProfilePicture", content, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            return;
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to upload profile picture: {responseContent}");
    }

    public async Task<StaffMemberDto> UpsertStaffMemberProfile(StaffMemberDto dto, CancellationToken cancellationToken)
    {
        var result = await _httpClient.PutAsJsonAsync($"{_baseRoute}", dto, cancellationToken);

        if  (result.IsSuccessStatusCode)
        {
            var response = await result.Content.ReadFromJsonAsync<StaffMemberDto>(cancellationToken: cancellationToken);

            if (response is not null)
            {
                return response;
            }
        }

        var responseContent = await result.Content.ReadAsStringAsync(cancellationToken);
        throw new Exception($"Failed to upsert staff member profile: {responseContent}");
    }
}