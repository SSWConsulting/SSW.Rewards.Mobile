namespace SSW.Rewards.Application.Achievements.Command.ClaimAchievementForUser
{
    public class ClaimAchievementResult
    {
        public ClaimAchievementStatus status { get; set; }
    }

    public enum ClaimAchievementStatus
    {
        Claimed,
        NotFound,
        Duplicate,
        NotEnoughPoints,
        Error
    }
}