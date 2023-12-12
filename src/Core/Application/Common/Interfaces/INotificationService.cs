using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface INotificationService
{
    Task CreateOrUpdateInstallationAsync(DeviceInstall deviceInstallation, CancellationToken token);
    Task DeleteInstallationByIdAsync(string installationId, CancellationToken token);
    Task RequestNotificationAsync(NotificationRequest notificationRequest, CancellationToken token);
}
