using AutoMapper;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards;

public class UserRewardDto : IMapFrom<UserReward>
{
    public string RewardName { get; set; }
    public int RewardCost { get; set; }
    public bool Awarded { get; set; }
    public DateTime? AwardedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserReward, UserRewardDto>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Awarded, opt => opt.MapFrom(src => src != null));
    }
}
