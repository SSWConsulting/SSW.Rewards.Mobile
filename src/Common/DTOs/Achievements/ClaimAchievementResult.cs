namespace Shared.DTOs.Achievements;

public class ClaimAchievementResult
{
    public AchievementDto? viewModel { get; set; }

    public ClaimAchievementStatus status { get; set; }
}