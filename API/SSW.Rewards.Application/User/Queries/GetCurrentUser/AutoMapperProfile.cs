using AutoMapper;
using System;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;

namespace SSW.Rewards.Application.User.Queries.GetCurrentUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile(ILogger<AutoMapperProfile> logger)
        {
            try
            {
                CreateMap<Domain.Entities.User, CurrentUserViewModel>()
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar));
            }
			catch (Exception e)
            {
                logger.LogError(e, "Unable to auto map {srcType} to {dstType}", typeof(Domain.Entities.User).Name, typeof(CurrentUserViewModel).Name);
                throw;
            }
        }
    }
}
