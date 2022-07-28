using SSW.Rewards.Application.Rewards.Common;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardList;
public class RewardListViewModel
{
    public IEnumerable<RewardViewModel> Rewards { get; set; } = new List<RewardViewModel>();
}