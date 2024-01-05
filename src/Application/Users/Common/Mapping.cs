using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Common;
public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<UserAchievement, UserAchievementDto>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Complete, opt => opt.MapFrom(src => src != null));
    }
}
