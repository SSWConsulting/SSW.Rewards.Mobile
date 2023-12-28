using Shared.DTOs.Achievements;

namespace Shared.Interfaces;

public interface IAchievementService
{
    Task<ClaimAchievementResult> ClaimAchievement(string code);
    Task<ClaimAchievementResult> ClaimAchievementForUser(string code, int userId);

    Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement);
    Task UpdateAchievement(int id, AchievementEditDto achievement);

    Task DeleteAchievement(int id);


    Task<AchievementAdminListViewModel> GetAdminAchievementList();
    Task<AchievementListViewModel> GetAchievementList();

    Task<AchievementUsersViewModel> GetAchievementUsers(int id);

    Task<AchievementListViewModel> SearchAchievements(string searchTerm);
}
