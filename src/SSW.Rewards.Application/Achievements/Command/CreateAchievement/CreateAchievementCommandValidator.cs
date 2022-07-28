namespace SSW.Rewards.Application.Achievements.Command.PostAchievement;

public class CreateAchievementCommandValidator : AbstractValidator<CreateAchievementCommand>
{
    public CreateAchievementCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Value).NotEmpty().LessThan(1000);
    }
}
