using SSW.Rewards.Models;

namespace SSW.Rewards.Services
{
    public interface IDeviceInstallationService
    {
        string Token { get; set; }
        bool NotificationsSupported { get; }
        string GetDeviceId();
        DeviceInstall GetDeviceInstallation(params string[] tags);
    }
}