namespace SSW.Rewards.Domain.Entities;
public class UnclaimedAchievement : BaseEntity
{
    public int AchievementId { get; set; }
    public Achievement Achievement { get; set; }

    public string EmailAddress { get; set; }
}
