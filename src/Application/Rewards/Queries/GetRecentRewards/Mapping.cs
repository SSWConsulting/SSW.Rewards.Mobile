using Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<UserReward, RecentRewardDto>()
                .ForMember(dst => dst.AwardedTo, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dst => dst.RewardName, opt => opt.MapFrom(src => src.Reward.Name))
                .ForMember(dst => dst.RewardCost, opt => opt.MapFrom(src => src.Reward.Cost));
    }
}
