namespace SSW.Rewards.Application.Notifications.Commands;

public class SendNotificationToUserCommandValidator : AbstractValidator<SendNotificationToUserCommand>
{
    public SendNotificationToUserCommandValidator()
    {
        RuleFor(x => x.UserId)
            .GreaterThan(0).WithMessage("UserId must be greater than 0.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Title cannot be whitespace.");

        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("Message is required.")
            .Must(t => !string.IsNullOrWhiteSpace(t)).WithMessage("Message cannot be whitespace.");
    }
}
