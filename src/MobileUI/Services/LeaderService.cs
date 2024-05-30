using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.Leaderboard;

namespace SSW.Rewards.Mobile.Services;

public interface ILeaderService
{
    Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh);
}

public class LeaderService : ILeaderService
{
    private readonly ILeaderboardService _leaderBoardClient;

    public LeaderService(ILeaderboardService leaderBoardClient)
    {
        _leaderBoardClient = leaderBoardClient;
    }

    public async Task<IEnumerable<LeaderboardUserDto>> GetLeadersAsync(bool forceRefresh)
    {
        List<LeaderboardUserDto> summaries = [];

        try
        {
            var apiLeaderList = await _leaderBoardClient.GetLeaderboard(CancellationToken.None);

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
