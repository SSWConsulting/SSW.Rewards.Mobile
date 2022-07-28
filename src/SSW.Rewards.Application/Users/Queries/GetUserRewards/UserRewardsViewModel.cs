using AutoMapper;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards;

public class UserRewardsViewModel : IMapFrom<User>
{
    public int UserId { get; set; }
    public IEnumerable<UserRewardDto> UserRewards { get; set; } = new List<UserRewardDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserRewardsViewModel>()
                .ForMember(dst => dst.UserId, opt => opt.MapFrom(src => src.Id))
                .ForMember(dst => dst.UserRewards, opt => opt.MapFrom(src => src.UserRewards));
    }
}
