using AutoMapper;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Application.Users.Queries.GetCurrentUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
			CreateMap<Domain.Entities.User, CurrentUserViewModel>()
				.ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
				.ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar));
		}
    }
}
