using SSW.Rewards.Application.Common.Mappings;

namespace SSW.Rewards.Application.Rewards.Common;

public class RewardViewModel : IMapFrom<Reward>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string ImageUri { get; set; }
    public RewardType RewardType { get; set; }
}
