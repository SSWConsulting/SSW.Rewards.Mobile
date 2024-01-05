using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.Shared.Services;

public interface ISkillsAdminService
{
    Task DeleteSkill(int skillId, CancellationToken cancellationToken);

    Task<int> AddOrUpdateSkill(SkillDto skill, CancellationToken cancellationToken);
}
