using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.AdminGetProfileDeletionRequests;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<OpenProfileDeletionRequest, ProfileDeletionRequestDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dest => dest.Requested, opt => opt.MapFrom(src => src.Created.ToString("O")))
            .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId));
    }
}
