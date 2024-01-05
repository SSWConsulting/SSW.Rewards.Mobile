namespace SSW.Rewards.Shared.DTOs.Users;

public class UserRewardsViewModel
{
    public int UserId { get; set; }
    public IEnumerable<UserRewardDto> UserRewards { get; set; } = Enumerable.Empty<UserRewardDto>();
}
