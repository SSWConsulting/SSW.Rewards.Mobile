using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetCurrentUser;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<User, CurrentUserDto>()
            .ForMember(dst => dst.Points, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value)))
            .ForMember(dst => dst.Balance, opt => opt.MapFrom(src => src.UserAchievements.Sum(ua => ua.Achievement.Value) - src.UserRewards.Sum(ur => ur.Reward.Cost)))
            .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar))
            .ForMember(dst => dst.QRCode, opt => opt.MapFrom((src, dst) => src?.Achievement?.Code?? string.Empty));
    }
}
