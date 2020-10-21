using AutoMapper;
using System;

namespace SSW.Rewards.Application.User.Queries.GetUserRewards
{
    internal class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<SSW.Rewards.Domain.Entities.UserReward, UserRewardViewModel>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Awarded, opt => opt.MapFrom(src => src != null));
        }
    }
}