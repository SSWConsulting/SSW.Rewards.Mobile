using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<User, NewUserDto>()
            .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.FullName))
            .ForMember(dst => dst.Email, opt => opt.MapFrom(src => src.Email));
    }
}