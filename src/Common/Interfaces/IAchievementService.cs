using Shared.DTOs.Achievements;

namespace Shared.Interfaces;

public interface IAchievementService
{
    Task<ClaimAchievementResult> ClaimAchievement(string code, CancellationToken cancellationToken);
    Task<ClaimAchievementResult> ClaimAchievementForUser(string code, int userId, CancellationToken cancellationToken);

    
    Task<AchievementListViewModel> GetAchievementList(CancellationToken cancellationToken);

    Task<AchievementUsersViewModel> GetAchievementUsers(int id, CancellationToken cancellationToken);

    Task<AchievementListViewModel> SearchAchievements(string searchTerm, CancellationToken cancellationToken);
}
