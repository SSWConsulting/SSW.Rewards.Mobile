using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;
public class RecentRewardListViewModel
{
    public IEnumerable<RecentRewardDto> Rewards { get; set; } = new List<RecentRewardDto>();
}

public class RecentRewardDto : IMapFrom<UserReward>
{
    public string RewardName { get; set; } = string.Empty;
    public int RewardCost { get; set; }
    public string AwardedTo { get; set; } = string.Empty;
    public DateTime AwardedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserReward, RecentRewardDto>()
                .ForMember(dst => dst.AwardedTo, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dst => dst.RewardName, opt => opt.MapFrom(src => src.Reward.Name))
                .ForMember(dst => dst.RewardCost, opt => opt.MapFrom(src => src.Reward.Cost));
    }
}
