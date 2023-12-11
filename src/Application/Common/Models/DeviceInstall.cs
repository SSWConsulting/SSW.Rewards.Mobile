namespace SSW.Rewards.Application.Common.Models;

public class DeviceInstall
{
    public string InstallationId { get; set; }
    public string Platform { get; set; }
    public string PushChannel { get; set; }
    public IList<string> Tags { get; set; } = Array.Empty<string>();
}
