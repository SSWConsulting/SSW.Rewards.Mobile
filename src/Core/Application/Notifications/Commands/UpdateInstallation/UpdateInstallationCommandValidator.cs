namespace SSW.Rewards.Application.Notifications.Commands.UpdateInstallation;

public class UpdateInstallationCommandValidator : AbstractValidator<UpdateInstallationCommand>
{
	public UpdateInstallationCommandValidator()
	{
		RuleFor(c => c.PushChannel)
			.NotEmpty();

		RuleFor(c => c.InstallationId)
			.NotEmpty();

		RuleFor(c => c.Platform)
			.NotEmpty();
	}
}
