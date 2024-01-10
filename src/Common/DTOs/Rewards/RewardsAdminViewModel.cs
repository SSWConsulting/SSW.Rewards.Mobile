namespace SSW.Rewards.Shared.DTOs.Rewards;
public class RewardsAdminViewModel
{
    public ICollection<RewardEditDto> Rewards { get; set; } = new List<RewardEditDto>();
}
