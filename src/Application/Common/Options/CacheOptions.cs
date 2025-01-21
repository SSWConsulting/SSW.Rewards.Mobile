namespace SSW.Rewards.Application.Common.Options;

public class CacheOptions
{
    public bool Enabled { get; set; }
    public TimeSpan DefaultExpiration { get; set; }
    public Dictionary<string, TimeSpan> OverrideExpiration { get; set; } = new();
}
