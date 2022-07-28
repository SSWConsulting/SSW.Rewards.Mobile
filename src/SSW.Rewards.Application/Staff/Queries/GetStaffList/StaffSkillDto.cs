using AutoMapper;
using SSW.Rewards.Application.Common.Mappings;
using SSW.Rewards.Domain.Entities;
using SSW.Rewards.Domain.Enums;

namespace SSW.Rewards.Application.Staff.Queries.GetStaffList;

public class StaffSkillDto : IMapFrom<StaffMemberSkill>
{
    public string Name { get; set; }
    public SkillLevel Level { get; set; }

    public void Mapping(Profile profile)
    {
        profile.CreateMap<StaffMemberSkill, StaffSkillDto>()
                .ForMember(dst => dst.Name, opt => opt.MapFrom(src => src.Skill.Name))
                .ForMember(dst => dst.Level, opt => opt.MapFrom(src => src.Level));
    }
}
