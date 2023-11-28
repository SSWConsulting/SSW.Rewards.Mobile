namespace SSW.Rewards.Application.Notifications.Commands.RequestNotification;
public class RequestNotificationCommandValidator : AbstractValidator<RequestNotificationCommand>
{
    public RequestNotificationCommandValidator()
    {
        RuleFor(c => c)
            .Must(HaveActionIfSilent)
            .Must(HaveTextIfNotSilent);

        RuleFor(c => c.Action)
            .NotEmpty();
    }

    private bool HaveActionIfSilent(RequestNotificationCommand command)
    {
        if (command.Silent && string.IsNullOrWhiteSpace(command.Action))
            return false;

        return true;
    }

    private bool HaveTextIfNotSilent(RequestNotificationCommand command)
    {
        if (!command.Silent && string.IsNullOrWhiteSpace(command.Text))
            return false;

        return true;
    }
}
