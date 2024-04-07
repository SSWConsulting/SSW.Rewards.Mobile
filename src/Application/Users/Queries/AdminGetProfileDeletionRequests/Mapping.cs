using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.AdminGetProfileDeletionRequests;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<OpenProfileDeletionRequest, ProfileDeletionRequestDto>()
            .ForMember(dst => dst.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dst => dst.Requested, opt => opt.MapFrom(src => src.CreatedUtc.ToLocalTime().ToShortDateString()));
    }
}
