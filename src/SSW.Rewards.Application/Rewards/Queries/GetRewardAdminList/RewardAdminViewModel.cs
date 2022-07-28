using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;

public class RewardAdminViewModel : IMapFrom<Reward>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string Code { get; set; }
}
