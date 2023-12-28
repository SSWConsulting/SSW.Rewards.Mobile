namespace Shared.DTOs.Achievements;

public class ClaimAchievementResult
{
    public AchievementDto? viewModel { get; set; }

    public AchievementStatus status { get; set; }
}

public enum AchievementStatus
{
    Added,
    NotFound,
    Duplicate,
    Error
}
