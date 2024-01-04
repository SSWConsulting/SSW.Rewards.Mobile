using Shared.DTOs.Achievements;

namespace Shared.Interfaces;

public interface IAchievementAdminService
{
    Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken);
    Task UpdateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken);

    Task DeleteAchievement(int id, CancellationToken cancellationToken);

    Task<AchievementAdminListViewModel> GetAdminAchievementList(CancellationToken cancellationToken);
}
