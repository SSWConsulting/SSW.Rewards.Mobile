using SSW.Rewards.Shared.DTOs.Roles;

namespace SSW.Rewards.Application.Roles.Queries.GetRoles;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<Role, RoleDto>()
            .ForMember(dst => dst.Id, opt => opt.MapFrom(src => src.Id))
            .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Name));
    }
}