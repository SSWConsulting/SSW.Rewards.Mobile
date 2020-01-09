using AutoMapper;
using System;
using System.Linq;

namespace SSW.Rewards.Application.User.Queries.GetCurrentUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.User, CurrentUserViewModel>()
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar == null ? null : new Uri(src.Avatar)));

        }
    }
}
