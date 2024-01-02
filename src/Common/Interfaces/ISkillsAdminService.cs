using Shared.DTOs.Skills;

namespace Shared.Interfaces;

public interface ISkillsAdminService
{
    Task DeleteSkill(string skillId);

    Task<int> AddOrUpdateSkill(SkillDto skill);
}
