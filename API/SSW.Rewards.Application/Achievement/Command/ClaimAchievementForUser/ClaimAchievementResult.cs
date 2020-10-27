namespace SSW.Rewards.Application.Achievement.Command.ClaimAchievementForUser
{
    public class ClaimAchievementResult
    {
        public AchievementStatus status { get; set; }
    }

    public enum AchievementStatus
    {
        Claimed,
        NotFound,
        Duplicate,
        NotEnoughPoints,
        Error
    }
}