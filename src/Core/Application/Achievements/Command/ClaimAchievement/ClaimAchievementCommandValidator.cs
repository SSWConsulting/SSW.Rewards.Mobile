namespace SSW.Rewards.Application.Achievements.Command.ClaimAchievement;

public class ClaimAchievementCommandValidator : AbstractValidator<ClaimAchievementCommand>
{
    public ClaimAchievementCommandValidator()
    {
        RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
    }
}
