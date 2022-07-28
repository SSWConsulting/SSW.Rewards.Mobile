using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Application.Users.Common;
using SSW.Rewards.Application.Users.Queries.GetUserRewards;

namespace SSW.Rewards.Application.Users.Queries.GetUser;

public class UserViewModel : IMapFrom<User>
{
    public int Id { get; set; }
    public string FullName { get; set; }
    public string ProfilePic { get; set; }
    public int Points { get; set; }
    public int Balance { get; set; }
    public IEnumerable<UserRewardDto> Rewards { get; set; }
    public IEnumerable<UserAchievementDto> Achievements { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, UserViewModel>()
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar));
    }
}