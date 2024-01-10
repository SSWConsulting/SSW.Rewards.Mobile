namespace SSW.Rewards.Shared.DTOs.Rewards;

public class RecentRewardDto
{
    public string RewardName { get; set; }
    public int RewardCost { get; set; }
    public string AwardedTo { get; set; }
    public DateTime AwardedAt { get; set; }
}
