﻿namespace SSW.Rewards.Application.Notifications.Commands.DeleteInstallation;
public class DeleteInstallation : IRequest<Unit>
{
    public string InstallationId { get; set; }

    public DeleteInstallation(string installationId)
    {
        InstallationId = installationId;
    }
}

public class DeleteInstallationHandler : IRequestHandler<DeleteInstallation, Unit>
{
    private readonly INotificationService _notificationService;

    public DeleteInstallationHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    public async Task<Unit> Handle(DeleteInstallation request, CancellationToken cancellationToken)
    {
        await _notificationService
            .DeleteInstallationByIdAsync(request.InstallationId, CancellationToken.None);

        return Unit.Value;
    }
}