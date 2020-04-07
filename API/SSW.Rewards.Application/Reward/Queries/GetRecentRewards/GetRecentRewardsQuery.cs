using MediatR;
using System;

namespace SSW.Rewards.Application.Reward.Queries.GetRecentRewards
{
    public class GetRecentRewardsQuery : IRequest<RecentRewardListViewModel>
    {
        public DateTime? Since { get; set; }
    }
}
