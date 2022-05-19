using AutoMapper;
using SSW.Rewards.Domain.Entities;
using System;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserReward, UserRewardViewModel>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Awarded, opt => opt.MapFrom(src => src != null));

            CreateMap<User, UserRewardsViewModel>();
                //.ForMember(dst => dst.UserRewards, opt => opt.MapFrom(src => src.UserRewards));
        }
    }
}