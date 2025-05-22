using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using System.Linq;

namespace SSW.Rewards.Mobile.Services;

public interface ILeaderService
{
    Task<MobileLeaderboardViewModel> GetLeadersAsync(bool forceRefresh, int page = 0, int pageSize = 50, LeaderboardFilter currentPeriod = LeaderboardFilter.Forever);
}

public class LeaderService : ILeaderService
{
    private readonly ILeaderboardService _leaderBoardClient;

    public LeaderService(ILeaderboardService leaderBoardClient)
    {
        _leaderBoardClient = leaderBoardClient;
    }

    public async Task<MobileLeaderboardViewModel> GetLeadersAsync(bool forceRefresh, int page = 0, int pageSize = 50, LeaderboardFilter currentPeriod = LeaderboardFilter.Forever)
    {
        MobileLeaderboardViewModel apiLeaderList = null;

        try
        {
            apiLeaderList = await _leaderBoardClient.GetMobilePaginatedLeaderboard(page, pageSize, currentPeriod, CancellationToken.None);

            // Update any empty profile pic.
            foreach (var leader in apiLeaderList.Items.Where(leader => string.IsNullOrWhiteSpace(leader.ProfilePic)))
            {
                leader.ProfilePic = "v2sophie";
            }
        }
        catch (Exception e)
        {
            if (!await ExceptionHandler.HandleApiException(e))
            {
                throw;
            }
        }

        return apiLeaderList;
    }
}
