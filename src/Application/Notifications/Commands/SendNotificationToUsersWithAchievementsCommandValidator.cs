namespace SSW.Rewards.Application.Notifications.Commands;

public class SendNotificationToUsersWithAchievementsCommandValidator : AbstractValidator<SendNotificationToUsersWithAchievementsCommand>
{
    public SendNotificationToUsersWithAchievementsCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Title cannot be whitespace.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Message cannot be whitespace.");

        RuleFor(x => x.AchievementIds)
            .NotNull().WithMessage("At least one achievement must be specified.")
            .Must(ids => ids != null && ids.Count > 0).WithMessage("At least one achievement must be specified.");
    }
}
