namespace SSW.Rewards.Application.Common.Models;
public class NotificationRequest
{
    public string Text { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public IList<string> Tags { get; set; } = new List<string>();
    public bool Silent { get; set; }
}
