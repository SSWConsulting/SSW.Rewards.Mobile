namespace SSW.Rewards.Mobile.Services;

/// <summary>Handles the interaction between the client and backend service.</summary>
public interface INotificationRegistrationService
{
    Task DeregisterDeviceAsync();
    Task RegisterDeviceAsync(params string[] tags);
    Task RefreshRegistrationAsync();
}