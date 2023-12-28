using Shared.DTOs.Achievements;

namespace Shared.Interfaces;

public interface IAchievementService
{
    Task<ClaimAchievementResult> ClaimAchievement(string code);
    Task<ClaimAchievementResult> ClaimAchievementForUser(string code, int userId);

    
    Task<AchievementListViewModel> GetAchievementList();

    Task<AchievementUsersViewModel> GetAchievementUsers(int id);

    Task<AchievementListViewModel> SearchAchievements(string searchTerm);
}
