using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Shared.DTOs.Staff;

public class StaffMemberDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Profile { get; set; } = string.Empty;

    public string TwitterUsername { get; set; } = string.Empty;
    public string GitHubUsername { get; set; } = string.Empty;
    public string LinkedInUrl { get; set; } = string.Empty;

    public string? ProfilePhoto { get; set; } 

    public int Points { get; set; }

    public bool IsDeleted { get; set; }

    public bool IsExternal { get; set; }

    public AchievementDto? StaffAchievement { get; set; }
    public bool Scanned { get; set; } = false;
    public ICollection<StaffSkillDto>? Skills { get; set; }
}
