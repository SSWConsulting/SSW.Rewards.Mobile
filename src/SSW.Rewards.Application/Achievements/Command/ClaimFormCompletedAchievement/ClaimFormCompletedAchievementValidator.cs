namespace SSW.Rewards.Application.Achievements.Command.ClaimFormCompletedAchievement;
public class ClaimFormCompletedAchievementValidator : AbstractValidator<ClaimFormCompletedAchievementCommand>
{
    public ClaimFormCompletedAchievementValidator()
    {
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.IntegrationId).NotEmpty();
    }
}
