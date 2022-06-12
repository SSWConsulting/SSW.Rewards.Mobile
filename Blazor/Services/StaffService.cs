using SSW.Rewards.Admin.Models.Staff;
using SSW.Rewards.Api;
using System.Net.Http.Json;
using StaffDto = SSW.Rewards.Api.StaffDto;

namespace SSW.Rewards.Admin.Services;

public class StaffService
{
    private readonly StaffClient _client;
    public StaffService(IHttpClientFactory clientFactory)
    {
        _client = new StaffClient(clientFactory.CreateClient(Constants.RewardsApiClient));
    }

    public async Task<StaffListViewModel?> GetAsync()
    {
        return await _client.GetAsync();
    }

    public async Task<StaffDto> GetStaffMemberProfileAsync(int id)
    {
        return await _client.GetStaffMemberProfileAsync(id);
    }

    public async Task UploadStaffMemberProfilePictureAsync(int? id, FileParameter file)
    {
        await _client.UploadStaffMemberProfilePictureAsync(id, file);
    }

    public async Task UpsertStaffMemberProfileAsync(UpsertStaffMemberProfileCommand command)
    {
        await _client.UpsertStaffMemberProfileAsync(command);
    }
    
    public static UpsertStaffMemberProfileCommand FromDto(StaffDto dto)
    {
        return new UpsertStaffMemberProfileCommand
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email,
            GitHubUsername = dto.GitHubUsername,
            LinkedInUrl = dto.LinkedInUrl,
            TwitterUsername = dto.TwitterUsername,
            Title = dto.Title,
            Profile = dto.Profile,
            ProfilePhoto = dto.ProfilePhoto != null ? new Uri(dto.ProfilePhoto) : null,
            Skills = dto.Skills,
        };
    }
}
