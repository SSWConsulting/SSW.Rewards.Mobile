namespace SSW.Rewards.Shared.DTOs.Notifications;

public class DeviceTokenDto
{
    public string DeviceToken { get; set; } = string.Empty;
    public DateTime LastTimeUpdated { get; set; }
    public string DeviceId { get; set; } = string.Empty;
}
