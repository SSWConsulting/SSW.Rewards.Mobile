using Shared.DTOs.Achievements;

namespace Shared.Interfaces;

public interface IAchievementAdminService
{
    Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken);
    Task UpdateAchievement(int id, AchievementEditDto achievement, CancellationToken cancellationToken);

    Task DeleteAchievement(int id, CancellationToken cancellationToken);

    Task<AchievementAdminListViewModel> GetAdminAchievementList(CancellationToken cancellationToken);
}
