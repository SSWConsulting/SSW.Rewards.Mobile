using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.Application.Skills.Queries.GetSkillList;

public class Mapping : Profile
{
    public Mapping()
    {
        CreateMap<Skill, SkillDto>();
    }
}
