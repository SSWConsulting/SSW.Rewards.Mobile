namespace SSW.Rewards.Application.Notifications.Commands.DeleteInstallation;
public class DeleteInstallationCommandValidator : AbstractValidator<DeleteInstallationCommand>
{
	public DeleteInstallationCommandValidator()
	{
		RuleFor(c => c.InstallationId)
			.NotEmpty();
	}
}
