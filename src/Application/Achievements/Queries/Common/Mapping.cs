using Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Queries.Common;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<Achievement, AchievementDto>();

        CreateMap<UserAchievement, AchievementUserDto>()
            .ForMember(dst => dst.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dst => dst.AwardedAtUtc, opt => opt.MapFrom(src => src.AwardedAt.ToUniversalTime()));
    }
}
