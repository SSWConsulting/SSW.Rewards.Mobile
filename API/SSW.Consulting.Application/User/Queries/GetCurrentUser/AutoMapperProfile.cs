using AutoMapper;
using System.Linq;

namespace SSW.Consulting.Application.User.Queries.GetCurrentUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Domain.Entities.User, CurrentUserViewModel>()
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)));
        }
    }
}
