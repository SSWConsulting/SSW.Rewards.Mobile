using AutoMapper;
using SSW.Rewards.Application.User.Queries.GetUser;
using SSW.Rewards.Application.User.Queries.GetUserAchievements;
using SSW.Rewards.Domain.Entities;
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
