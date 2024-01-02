namespace SSW.Rewards.Mobile.Services;

public interface IDeviceInstallationService
{
    string Token { get; set; }
    bool NotificationsSupported { get; }
    string GetDeviceId();
    DeviceInstall GetDeviceInstallation(params string[] tags);
}