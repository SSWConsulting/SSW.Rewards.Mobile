using AutoMapper;
using SSW.Rewards.Application.Users.Common;
using SSW.Rewards.Domain.Entities;
using System;

namespace SSW.Rewards.Application.Users.Queries.GetUserAchievements
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<UserAchievement, UserAchievementDto>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Complete, opt => opt.MapFrom(src => src != null));
        }
    }
}