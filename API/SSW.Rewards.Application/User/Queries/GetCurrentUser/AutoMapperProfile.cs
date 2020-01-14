using AutoMapper;
using System;
using System.Diagnostics;
using System.Linq;

namespace SSW.Rewards.Application.User.Queries.GetCurrentUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            try
            {
                CreateMap<Domain.Entities.User, CurrentUserViewModel>()
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar));
            }
            catch(Exception e)
            {
                Debug.Write(e);
            }
            

        }
    }
}
