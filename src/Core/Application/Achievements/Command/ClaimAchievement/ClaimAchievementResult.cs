using SSW.Rewards.Application.Achievements.Queries.Common;

namespace SSW.Rewards.Application.Achievements.Command.ClaimAchievement;

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
