using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Notifications.Commands.UpdateInstallation;
public class UpdateInstallation : IRequest<Unit>
{
    public string InstallationId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PushChannel { get; set; } = string.Empty;
    public IList<string> Tags { get; set; } = new List<string>();

    public UpdateInstallation(string installationId, string platform, string pushChannel, IList<string> tags)
    {
        InstallationId  = installationId;
        Platform        = platform;
        PushChannel     = pushChannel;
        Tags            = tags;
    }
}

public class UpdateInstallationHandler : IRequestHandler<UpdateInstallation, Unit>
{
    private readonly INotificationService _notificationService;

    public UpdateInstallationHandler(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }
    public async Task<Unit> Handle(UpdateInstallation request, CancellationToken cancellationToken)
    {
        var deviceInstallation = new DeviceInstall
        {
            InstallationId  = request.InstallationId,
            Platform        = request.Platform,
            PushChannel     = request.PushChannel,
            Tags            = request.Tags
        };

        await _notificationService
            .CreateOrUpdateInstallationAsync(deviceInstallation, cancellationToken);

        return Unit.Value;
    }
}