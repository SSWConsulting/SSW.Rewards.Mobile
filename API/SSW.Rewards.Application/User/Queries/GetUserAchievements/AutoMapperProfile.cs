using AutoMapper;
using SSW.Rewards.Domain.Entities;
using System;

namespace SSW.Rewards.Application.User.Queries.GetUserAchievements
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<JoinedUserAchievement, UserAchievementViewModel>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src.UserAchievement != null ? src.UserAchievement.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Complete, opt => opt.MapFrom(src => src.UserAchievement != null));
            CreateMap<UserAchievement, UserAchievementViewModel>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Complete, opt => opt.MapFrom(src => src != null));
        }
    }
}