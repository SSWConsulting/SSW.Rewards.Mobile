using FluentValidation;

namespace SSW.Rewards.Application.Achievement.Commands.AddAchievement
{
    public class AddAchievementCommandValidator : AbstractValidator<AddAchievementCommand>
    {
        public AddAchievementCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        }
    }
}
