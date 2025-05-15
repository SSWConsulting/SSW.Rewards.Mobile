using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Mobile.Services;

public interface ILeaderService
{
    Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh, int skip = 0, int take = 50, LeaderboardFilter currentPeriod = LeaderboardFilter.Forever);
}

public class LeaderService : ILeaderService
{
    private readonly ILeaderboardService _leaderBoardClient;

    public LeaderService(ILeaderboardService leaderBoardClient)
    {
        _leaderBoardClient = leaderBoardClient;
    }

    public async Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh, int skip = 0, int take = 50, LeaderboardFilter currentPeriod = LeaderboardFilter.Forever)
    {
        List<LeaderboardUserDto> summaries = [];

        try
        {
            var apiLeaderList = await _leaderBoardClient.GetPaginatedLeaderboard(take, skip, currentPeriod, CancellationToken.None);

            foreach (var leader in apiLeaderList.Users)
            {
                if (string.IsNullOrWhiteSpace(leader.ProfilePic))
                {
                    leader.ProfilePic = "v2sophie";
                }

                summaries.Add(leader);
            }
        }
        catch(Exception e)
        {
            if (!await ExceptionHandler.HandleApiException(e))
            {
                throw;
            }
        }

        return summaries;
    }
}
