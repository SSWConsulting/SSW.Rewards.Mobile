using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetUserPendingRedemptions;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<PendingRedemption, UserPendingRedemptionDto>()
                .ForMember(dst => dst.Code, opt => opt.MapFrom(src => src.Code))
                .ForMember(dst => dst.ClaimedAt, opt => opt.MapFrom(src => src.ClaimedAt))
                .ForMember(dst => dst.RewardId, opt => opt.MapFrom(src => src.RewardId));

        CreateMap<User, UserPendingRedemptionsViewModel>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.PendingRedemptions, opt => opt.MapFrom(src => src.PendingRedemptions));
    }
}
