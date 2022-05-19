using FluentValidation;

namespace SSW.Rewards.Application.Achievements.Commands.AddAchievement
{
    public class AddAchievementCommandValidator : AbstractValidator<AddAchievementCommand>
    {
        public AddAchievementCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        }
    }
}
