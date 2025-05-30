using SSW.Rewards.Admin.UI.Models;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.PrizeDraw;

namespace SSW.Rewards.Admin.UI.Services;

public interface IPrizeDrawManager
{
    Task<List<EligibleUserDto>> GetEligiblePlayersAsync(GetEligibleUsersFilter filter, List<Winner> previousWinners, bool excludePreviousWinners);
    EligibleUserDto? SelectRandomWinner(List<EligibleUserDto> players);
    (DateTime? from, DateTime? to) GetDateRangeForFilter(LeaderboardFilter filter, IDateTime dateTimeService);
}

public class PrizeDrawManager(IPrizeDrawService prizeDrawService) : IPrizeDrawManager
{
    public async Task<List<EligibleUserDto>> GetEligiblePlayersAsync(
        GetEligibleUsersFilter filter,
        List<Winner> previousWinners,
        bool excludePreviousWinners)
    {
        var eligibleUsersVm = await prizeDrawService.GetEligibleUsers(filter, CancellationToken.None);
        var eligiblePlayers = eligibleUsersVm.EligibleUsers.ToList();

        if (excludePreviousWinners)
        {
            var previousWinnerIds = previousWinners.Select(w => w.Id).ToHashSet();
            eligiblePlayers = eligiblePlayers
                .Where(p => !previousWinnerIds.Contains(p.UserId.ToString()))
                .ToList();
        }

        return eligiblePlayers;
    }

    public EligibleUserDto? SelectRandomWinner(List<EligibleUserDto> players)
    {
        if (players.Count == 0) return null;

        var rand = new Random();
        return players.ElementAt(rand.Next(players.Count));
    }

    public (DateTime? from, DateTime? to) GetDateRangeForFilter(LeaderboardFilter filter, IDateTime dateTimeService)
    {
        var now = dateTimeService.Now;

        (DateTime? localFrom, DateTime? localTo) = filter switch
        {
            LeaderboardFilter.Today => (now.Date, now.Date.AddDays(1).AddTicks(-1)),
            LeaderboardFilter.ThisWeek => (now.Date.AddDays(-(int)now.DayOfWeek),
                now.Date.AddDays(7 - (int)now.DayOfWeek).AddTicks(-1)),
            LeaderboardFilter.ThisMonth => (new DateTime(now.Year, now.Month, 1),
                new DateTime(now.Year, now.Month, 1).AddMonths(1).AddTicks(-1)),
            LeaderboardFilter.ThisYear => (new DateTime(now.Year, 1, 1),
                new DateTime(now.Year + 1, 1, 1).AddTicks(-1)),
            LeaderboardFilter.Forever => (null, null),
            _ => ((DateTime?)null, (DateTime?)null)
        };

        if (localFrom.HasValue && localTo.HasValue)
        {
            return (localFrom.Value.ToUniversalTime(), localTo.Value.ToUniversalTime());
        }

        return (null, null);
    }
}