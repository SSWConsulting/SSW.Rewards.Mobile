namespace Microsoft.Extensions.DependencyInjection.Notifications.Commands.UploadDeviceToken;

public class DeviceTokenDto
{
    public string DeviceToken { get; set; }
    public DateTime LastTimeUpdated { get; set; }
    public string DeviceId { get; set; }
}