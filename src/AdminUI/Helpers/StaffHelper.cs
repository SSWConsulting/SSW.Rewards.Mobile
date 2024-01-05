using SSW.Rewards.Shared.DTOs.Staff;

namespace SSW.Rewards.Admin.UI.Helpers;

public static class StaffHelper
{
    public static StaffMemberDto FromDto(StaffMemberDto dto)
    {
        return new StaffMemberDto
        {
            Id = dto.Id,
            Name = dto.Name,
            Email = dto.Email ?? string.Empty,
            GitHubUsername = dto.GitHubUsername ?? string.Empty,
            LinkedInUrl = dto.LinkedInUrl ?? string.Empty,
            TwitterUsername = dto.TwitterUsername ?? string.Empty,
            Title = dto.Title ?? string.Empty,
            Profile = dto.Profile ?? string.Empty,
            ProfilePhoto = dto.ProfilePhoto,
            Skills = (ICollection<StaffSkillDto>)dto.Skills,
            Points = dto.StaffAchievement?.Value ?? 0,
        };
    }
}
