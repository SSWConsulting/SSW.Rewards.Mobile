using SSW.Rewards.Application.Common.Mappings;

namespace SSW.Rewards.Application.Users.Queries.GetCurrentUser;

public class CurrentUserViewModel : IMapFrom<User>
{
    public int Id { get; set; }

    public string Email { get; set; }

    public string FullName { get; set; }

    public string ProfilePic { get; set; }

    public int Points { get; set; }

    public int Balance { get; set; }

    public string QRCode { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<User, CurrentUserViewModel>()
                .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
                .ForMember(dst => dst.Balance, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value) - src.UserRewards.Sum(ur => ur.Reward.Cost)))
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dst => dst.QRCode, opt => opt.Ignore());
    }
}
