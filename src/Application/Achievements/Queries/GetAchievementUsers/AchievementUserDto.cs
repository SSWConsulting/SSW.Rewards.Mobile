using SSW.Rewards.Application.Common.Mappings;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementUsers;

public class AchievementUserDto : IMapFrom<UserAchievement>
{
    public int UserId { get; set; }

    public string UserName { get; set; }

    public string UserEmail { get; set; }

    public DateTime AwardedAtUtc { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserAchievement, AchievementUserDto>()
            .ForMember(dst => dst.UserName, opt => opt.MapFrom(src => src.User.FullName))
            .ForMember(dst => dst.AwardedAtUtc, opt => opt.MapFrom(src => src.AwardedAt.ToUniversalTime()));
    }
}