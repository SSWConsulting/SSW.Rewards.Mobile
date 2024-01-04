using Shared.DTOs.Skills;

namespace Shared.Interfaces;

public interface ISkillsAdminService
{
    Task DeleteSkill(string skillId, CancellationToken cancellationToken);

    Task<int> AddOrUpdateSkill(SkillDto skill, CancellationToken cancellationToken);
}
