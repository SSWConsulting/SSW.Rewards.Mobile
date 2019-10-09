using AutoMapper;
using System;
using System.Linq;

namespace SSW.Consulting.Application.User.Queries.GetUserAchievements
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<JoinedUserAchievement, UserAchievementViewModel>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src.UserAchievement != null ? src.UserAchievement.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Complete, opt => opt.MapFrom(src => src.UserAchievement != null));
        }
    }
}
