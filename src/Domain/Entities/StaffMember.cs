namespace SSW.Rewards.Domain.Entities;
public class StaffMember : BaseAuditableEntity
{
    public string? Name { get; set; } = string.Empty;
    public string? Title { get; set; } = string.Empty;
    public string? Email { get; set; } = string.Empty;
    public string? Profile { get; set; } = string.Empty;
    public string? TwitterUsername { get; set; } = string.Empty;
    public string? GitHubUsername { get; set; } = string.Empty;
    public string? LinkedInUrl { get; set; } = string.Empty;
    public bool IsExternal { get; set; }
    public int? StaffAchievementId { get; set; }
    public Achievement? StaffAchievement { get; set; }
    public ICollection<StaffMemberSkill> StaffMemberSkills { get; set; } = new HashSet<StaffMemberSkill>();
    public string? ProfilePhoto { get; set; } = string.Empty;
}
