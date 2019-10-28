using FluentValidation;

namespace SSW.Consulting.Application.Achievement.Commands.AddAchievement
{
    public class AddAchievementCommandValidator : AbstractValidator<AddAchievementCommand>
    {
        public AddAchievementCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        }
    }
}
