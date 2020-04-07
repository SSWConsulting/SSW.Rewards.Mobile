using AutoMapper;
using SSW.Rewards.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SSW.Rewards.Application.Reward.Queries.GetRecentRewards
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserReward, RecentRewardViewModel>()
                .ForMember(dst => dst.AwardedTo, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dst => dst.RewardName, opt => opt.MapFrom(src => src.Reward.Name))
                .ForMember(dst => dst.RewardCost, opt => opt.MapFrom(src => src.Reward.Cost));
        }
    }
}
