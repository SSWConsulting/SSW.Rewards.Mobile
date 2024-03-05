using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Mobile.Services;

public class DevService : IDevService
{
    private readonly IStaffService _staffClient;

    public DevService(IStaffService staffClient)
    {
        _staffClient = staffClient;
    }

    public async Task<IEnumerable<DevProfile>> GetProfilesAsync()
    {
		List<DevProfile> profiles = new(200);
        int id = 0;

        try
        {
            var profileList = await _staffClient.GetStaffList(CancellationToken.None);

            foreach (var profile in profileList.Staff.Where(s => !s.IsDeleted))
            {
                var dev = new DevProfile
                {
                    id = id,
                    FirstName = profile.Name,
                    Bio = profile.Profile,
                    Email = profile.Email,
                    Picture = string.IsNullOrWhiteSpace(profile.ProfilePhoto?.ToString())
                        ? "dev_placeholder"
                        : profile.ProfilePhoto.ToString(),
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

                profiles.Add(dev);
                id++;
            }
        }
        catch (Exception e)
        {
            if (!await ExceptionHandler.HandleApiException(e))
            {
                throw;
            }
        }

        return profiles.OrderBy(d => d.FirstName);
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
