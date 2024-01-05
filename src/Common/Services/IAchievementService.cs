using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.Shared.Services;

public interface IAchievementService
{
    Task<ClaimAchievementResult> ClaimAchievement(string code, CancellationToken cancellationToken);
    
    Task<AchievementListViewModel> GetAchievementList(CancellationToken cancellationToken);

    Task<AchievementUsersViewModel> GetAchievementUsers(int id, CancellationToken cancellationToken);

    Task<AchievementListViewModel> SearchAchievements(string searchTerm, CancellationToken cancellationToken);
}
