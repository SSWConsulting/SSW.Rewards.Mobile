using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.Staff;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.Services;

public interface IDevService
{
    Task<IEnumerable<NetworkProfileDto>> GetProfilesAsync();
    
    Task<DevProfile> GetProfileAsync(string email);
}

public class DevService : IDevService
{
    private readonly IStaffService _staffClient;

    public DevService(IStaffService staffClient)
    {
        _staffClient = staffClient;
    }

    public async Task<IEnumerable<NetworkProfileDto>> GetProfilesAsync()
    {
        try
        {
            var vm = await _staffClient.GetNetworkProfileList(CancellationToken.None);

            return vm.Profiles
                .Select(x =>
                {
                    x.ProfilePicture = string.IsNullOrWhiteSpace(x.ProfilePicture)
                        ? "v2sophie"
                        : x.ProfilePicture;
                    return x;
                })
                .ToList()
                .OrderBy(x => x.Name);
        }
        catch (Exception e)
        {
            if (!await ExceptionHandler.HandleApiException(e))
            {
                throw;
            }
        }
        
        return new List<NetworkProfileDto>(0);
    }
    
    public async Task<DevProfile> GetProfileAsync(string email)
    {
        try
        {
            var profile =
                await _staffClient.SearchStaffMember(new StaffMemberQueryDto() { email = email, GetByEmail = true },
                    CancellationToken.None);

            return new DevProfile
            {
                id = profile.Id,
                FirstName = profile.Name,
                Bio = profile.Profile,
                Email = profile.Email,
                Picture = string.IsNullOrWhiteSpace(profile.ProfilePhoto)
                    ? "dev_placeholder"
                    : profile.ProfilePhoto,
                Title = profile.Title,
                TwitterID = profile.TwitterUsername,
                GitHubID = profile.GitHubUsername,
                LinkedInId = profile.LinkedInUrl,
                Skills = profile.Skills?.ToList(),
                IsExternal = profile.IsExternal,
                AchievementId = profile.StaffAchievement?.Id ?? 0,
                Scanned = profile.Scanned,
                Points = profile.StaffAchievement?.Value ?? 0
            };
        }
        catch (Exception e)
        {
            await ExceptionHandler.HandleApiException(e);
        }

        return null;
    }
}
