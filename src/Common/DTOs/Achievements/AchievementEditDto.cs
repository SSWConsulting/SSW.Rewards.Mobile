using Shared.Models;

namespace Shared.DTOs.Achievements;

public class AchievementEditDto
{
    public string Name { get; set; }
    public int Value { get; set; }
    public AchievementType Type { get; set; }

    public bool IsMultiscanEnabled { get; set; }
}
