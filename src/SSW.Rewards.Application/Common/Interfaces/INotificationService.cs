using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Common.Interfaces;
public interface INotificationService
{
    Task<bool> CreateOrUpdateInstallationAsync(DeviceInstall deviceInstallation, CancellationToken token);
    Task<bool> DeleteInstallationByIdAsync(string installationId, CancellationToken token);
    Task<bool> RequestNotificationAsync(NotificationRequest notificationRequest, CancellationToken token);
}
