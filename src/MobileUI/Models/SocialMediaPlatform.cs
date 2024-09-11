namespace SSW.Rewards.Models;

public class SocialMediaPlatform
{
    public int PlatformId { get; set; }
    public string PlatformName { get; set; }
    public string Url { get; set; }
    public string Placeholder { get; set; }
    public string ValidationPattern { get; set; }
}