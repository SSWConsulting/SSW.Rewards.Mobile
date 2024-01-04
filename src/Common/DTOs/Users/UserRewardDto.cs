namespace Shared.DTOs.Users;

public class UserRewardDto
{
    public required string RewardName { get; set; }
    public int RewardCost { get; set; }
    public bool Awarded { get; set; }
    public DateTime? AwardedAt { get; set; }
}
