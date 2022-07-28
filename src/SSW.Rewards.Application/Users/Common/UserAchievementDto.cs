using AutoMapper;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Domain.Enums;

namespace SSW.Rewards.Application.Users.Common;

public class UserAchievementDto : IMapFrom<UserAchievement>
{
    public int AchievementId { get; set; }
    
    public string AchievementName { get; set; }
    
    public int AchievementValue { get; set; }
    
    public bool Complete { get; set; }
    
    public AchievementType AchievementType { get; set; }

    public Icons AchievementIcon { get; set; }

    public bool AchievementIconIsBranded { get; set; }

    public DateTime? AwardedAt { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<UserAchievement, UserAchievementDto>()
                .ForMember(dst => dst.AwardedAt, opt => opt.MapFrom(src => src != null ? src.AwardedAt : (DateTime?)null))
                .ForMember(dst => dst.Complete, opt => opt.MapFrom(src => src != null));
    }
}
