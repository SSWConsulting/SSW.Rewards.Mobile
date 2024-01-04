using Shared.DTOs.Achievements;

namespace Shared.DTOs.Staff;

public class StaffMemberDto
{
    public int Id { get; set; }
    public required string Name { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;

    public string TwitterUsername { get; set; } = string.Empty;
    public string GitHubUsername { get; set; } = string.Empty;
    public string LinkedInUrl { get; set; } = string.Empty;

    public string? ProfilePhoto { get; set; } 

    public int Points { get; set; }

    public AchievementDto? StaffAchievement { get; set; }
    public bool Scanned { get; set; } = false;
    public IEnumerable<StaffSkillDto> Skills { get; set; } = Enumerable.Empty<StaffSkillDto>();
}
