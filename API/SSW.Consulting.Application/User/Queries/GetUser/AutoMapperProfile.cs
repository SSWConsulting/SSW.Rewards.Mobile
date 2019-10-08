using AutoMapper;
using SSW.Consulting.Application.User.Queries.GetUser;
using SSW.Consulting.Application.User.Queries.GetUserAchievements;
using SSW.Consulting.Domain.Entities;
using System.Linq;

namespace HW.KNOWnoise.Application.Admin.Users.Queries.GetUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)));
        }
    }
}
