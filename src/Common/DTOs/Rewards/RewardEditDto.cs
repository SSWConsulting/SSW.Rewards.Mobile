

namespace Shared.DTOs.Rewards;

public class RewardEditDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string ImageUri { get; set; }
    public RewardType RewardType { get; set; }
    public string ImageBytesInBase64 { get; set; }
    public string ImageFileName { get; set; }
    public bool? IsOnboardingReward { get; set; }
}
