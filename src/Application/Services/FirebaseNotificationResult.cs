namespace SSW.Rewards.Application.Services;

public class FirebaseNotificationResult
{
    public int Devices { get; set; }
    public int Sent { get; set; }
    public int Failed { get; set; }
    public int Removed { get; set; }
}
