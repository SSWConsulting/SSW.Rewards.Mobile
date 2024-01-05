using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.ApiClient.Services;

public interface ISkillsAdminService
{
    Task DeleteSkill(int skillId, CancellationToken cancellationToken);

    Task<int> AddOrUpdateSkill(SkillDto skill, CancellationToken cancellationToken);
}
