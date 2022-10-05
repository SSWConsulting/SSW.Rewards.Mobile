using SSW.Rewards.Application.Common.Mappings;

namespace SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;

public class AchievementAdminViewModel : IMapFrom<Achievement>
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Value { get; set; }
    public string Code { get; set; }
    public AchievementType Type { get; set; }
    public bool? IsArchived { get; set; }
    public bool? IsMultiScanEnabled { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<Achievement, AchievementAdminViewModel>()
                .ForMember(dst => dst.IsArchived, opt => opt.MapFrom(src => src.IsDeleted));
    }
}
