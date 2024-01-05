using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.Shared.Services;

public interface ISkillsService
{
    Task<SkillsListViewModel> GetSkillsList(CancellationToken cancellationToken);
}
