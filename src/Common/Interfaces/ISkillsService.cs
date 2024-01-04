using Shared.DTOs.Skills;

namespace Shared.Interfaces;

public interface ISkillsService
{
    Task<SkillsListViewModel> GetSkillsList(CancellationToken cancellationToken);
}
