using SSW.Rewards.Application.Common.Mappings;

namespace SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;

public class RecentRewardViewModel : IMapFrom<UserReward>
{
    public string RewardName { get; set; }
    public int RewardCost { get; set; }
    public string AwardedTo { get; set; }
    public DateTime AwardedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserReward, RecentRewardViewModel>()
                .ForMember(dst => dst.AwardedTo, opt => opt.MapFrom(src => src.User.FullName))
                .ForMember(dst => dst.RewardName, opt => opt.MapFrom(src => src.Reward.Name))
                .ForMember(dst => dst.RewardCost, opt => opt.MapFrom(src => src.Reward.Cost));
    }
}
