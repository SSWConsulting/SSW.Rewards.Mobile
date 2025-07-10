using System.Text.RegularExpressions;

namespace SSW.Rewards.Models;

public class SocialMediaPlatform
{
    public string PlatformName { get; set; }
    public string Url { get; set; }
    public string Placeholder { get; set; }
    public Func<Regex> ValidationPattern { get; set; }
    public string Icon { get; set; }
}