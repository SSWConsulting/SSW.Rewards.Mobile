namespace SSW.Rewards.Shared.DTOs.Users;

public class UserPendingRedemptionsViewModel
{
    public int UserId { get; set; }
    public IEnumerable<UserPendingRedemptionDto> PendingRedemptions { get; set; } = Enumerable.Empty<UserPendingRedemptionDto>();
}
