using Shared.Models;

namespace Shared.DTOs.Achievements;

public class AchievementAdminDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public string Code { get; set; }
    public AchievementType Type { get; set; }
    public bool? IsArchived { get; set; }
    public bool? IsMultiScanEnabled { get; set; }
    public string? IntegrationId { get; set; }
}
