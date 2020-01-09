using AutoMapper;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.User.Commands.UpsertCurrentUser
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<ICurrentUserService, Domain.Entities.User>()
                .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.GetUserEmail()))
                .ForMember(dst => dst.FullName, opt => opt.MapFrom(src => src.GetUserFullName()))
                .ForMember(dst => dst.Avatar, opt => opt.MapFrom(src => src.GetUserProfilePic()));
        }
    }
}
