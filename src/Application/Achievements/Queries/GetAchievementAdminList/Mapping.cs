using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<Achievement, AchievementAdminDto>()
            .ForMember(dst => dst.IsArchived, opt => opt.MapFrom(src => src.DeletedUtc != null));
    }
}
