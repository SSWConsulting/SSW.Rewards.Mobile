using AutoMapper;
using SSW.Rewards.Application.Users.Queries.GetUser;
using SSW.Rewards.Domain.Entities;

namespace HW.KNOWnoise.Application.Admin.Users.Queries.GetUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<User, UserViewModel>()
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar));
        }
    }
}
