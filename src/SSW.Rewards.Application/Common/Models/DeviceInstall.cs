namespace SSW.Rewards.Application.Common.Models;
public class DeviceInstall
{
    public string InstallationId { get; set; } = string.Empty;
    public string Platform { get; set; } = string.Empty;
    public string PushChannel { get; set; } = string.Empty;
    public IList<string> Tags { get; set; } = new List<string>();
}
