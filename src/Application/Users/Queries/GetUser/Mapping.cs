using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Queries.GetUser;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<User, UserProfileDto>()
                .ForMember(dst => dst.ProfilePic, opt => opt.MapFrom(src => src.Avatar))
                .ForMember(dst => dst.Points, opt => opt.Ignore())
                .ForMember(dst => dst.Balance, opt => opt.Ignore())
                .ForMember(dst => dst.Rewards, opt => opt.MapFrom(src => src.UserRewards))
                .ForMember(dst => dst.Achievements, opt => opt.MapFrom(src => src.UserAchievements));
    }
}
