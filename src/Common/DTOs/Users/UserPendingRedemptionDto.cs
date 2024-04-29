namespace SSW.Rewards.Shared.DTOs.Users;

public class UserPendingRedemptionDto
{
    public string? Code { get; set; }
    public DateTime ClaimedAt { get; set; }
    public int RewardId { get; set; }
}
