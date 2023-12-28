using Shared.DTOs.Achievements;

namespace Shared.Interfaces;

public interface IAchievementAdminService
{
    Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement);
    Task UpdateAchievement(int id, AchievementEditDto achievement);

    Task DeleteAchievement(int id);

    Task<AchievementAdminListViewModel> GetAdminAchievementList();
}
