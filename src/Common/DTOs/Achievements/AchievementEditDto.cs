

namespace Shared.DTOs.Achievements;

public class AchievementEditDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public AchievementType Type { get; set; }

    public bool IsMultiscanEnabled { get; set; }
}
