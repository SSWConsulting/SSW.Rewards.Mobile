using SSW.Rewards.Shared.DTOs.Achievements;

namespace SSW.Rewards.ApiClient.Services;

public interface IAchievementAdminService
{
    Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken);

    Task UpdateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken);

    Task DeleteAchievement(int id, CancellationToken cancellationToken);

    Task<AchievementAdminListViewModel> GetAdminAchievementList(CancellationToken cancellationToken);

    Task<ClaimAchievementResult> ClaimAchievementForUser(string code, int userId, CancellationToken cancellationToken);
}

public class AchievementAdminService : IAchievementAdminService
{
    private readonly HttpClient _httpClient;

    public AchievementAdminService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient(Constants.AuthenticatedClient);
    }

    public Task<ClaimAchievementResult> ClaimAchievementForUser(string code, int userId, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AchievementAdminDto> CreateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task DeleteAchievement(int id, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task<AchievementAdminListViewModel> GetAdminAchievementList(CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task UpdateAchievement(AchievementEditDto achievement, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
