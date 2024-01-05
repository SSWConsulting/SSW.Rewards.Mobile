using SSW.Rewards.Shared.DTOs.Skills;

namespace SSW.Rewards.ApiClient.Services;

public interface ISkillsService
{
    Task<SkillsListViewModel> GetSkillsList(CancellationToken cancellationToken);
}
