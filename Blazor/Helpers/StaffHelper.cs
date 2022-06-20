namespace SSW.Rewards.Admin.Helpers
{
    public static class StaffHelper
    {
        public static UpsertStaffMemberProfileCommand FromDto(StaffDto dto)
        {
            return new UpsertStaffMemberProfileCommand
            {
                Id = dto.Id,
                Name = dto.Name,
                Email = dto.Email ?? string.Empty,
                GitHubUsername = dto.GitHubUsername ?? string.Empty,
                LinkedInUrl = dto.LinkedInUrl ?? string.Empty,
                TwitterUsername = dto.TwitterUsername ?? string.Empty,
                Title = dto.Title ?? string.Empty,
                Profile = dto.Profile ?? string.Empty,
                ProfilePhoto = dto.ProfilePhoto != null ? new Uri(dto.ProfilePhoto) : null,
                Skills = (ICollection<StaffSkillDto>)dto.Skills,
                Points = dto.StaffAchievement.Value,
            };
        }
    }
}
