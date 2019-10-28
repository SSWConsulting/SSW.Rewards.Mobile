using FluentValidation;

namespace SSW.Consulting.Application.Achievement.Command.PostAchievement
{
    public class PostAchievementCommandValidator : AbstractValidator<PostAchievementCommand>
    {
        public PostAchievementCommandValidator()
        {
            RuleFor(x => x.Code).NotEmpty().MaximumLength(128);
        }
    }
}
