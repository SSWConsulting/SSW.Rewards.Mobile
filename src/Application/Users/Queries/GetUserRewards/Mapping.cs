using Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<UserReward, UserRewardDto>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Awarded, opt => opt.MapFrom(src => src != null));

        CreateMap<User, UserRewardsViewModel>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.UserRewards, opt => opt.MapFrom(src => src.UserRewards));
    }
}
