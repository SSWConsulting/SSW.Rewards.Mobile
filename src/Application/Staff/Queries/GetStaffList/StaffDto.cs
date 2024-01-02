using SSW.Rewards.Application.Achievements.Queries.Common;
using SSW.Rewards.Application.Common.Mappings;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList;

public class StaffDto : IMapFrom<StaffMember>
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Title { get; set; }
    public string? Email { get; set; }
    public string? Profile { get; set; }
    public string? ProfilePhoto { get; set; }
    public bool IsDeleted { get; set; }
    public string? TwitterUsername { get; set; }
    public string? GitHubUsername { get; set; }
    public string? LinkedInUrl { get; set; }
    public bool IsExternal { get; set; }
    public AchievementDto? StaffAchievement { get; set; }
    public bool Scanned { get; set; } = false;
    public IEnumerable<StaffSkillDto> Skills { get; set; } = Enumerable.Empty<StaffSkillDto>();

    public void Mapping(Profile profile)
    {
        profile.CreateMap<StaffMember, StaffDto>()
                .ForMember(dest => dest.Skills, opt => opt.MapFrom(src => src.StaffMemberSkills))
                .ForMember(dest => dest.Scanned, opt => opt.Ignore());
    }
}
