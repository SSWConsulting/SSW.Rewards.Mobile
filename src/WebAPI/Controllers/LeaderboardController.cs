using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardPaginatedList;
using SSW.Rewards.Application.Leaderboard.Queries.GetMobileLeaderboard;
using SSW.Rewards.Application.PrizeDraw.Queries;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Shared.DTOs.PrizeDraw;

namespace SSW.Rewards.WebAPI.Controllers;

public class LeaderboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LeaderboardViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetLeaderboardListQuery()));
    }

    [HttpGet]
    public async Task<ActionResult<LeaderboardViewModel>> GetPaginated([FromQuery] int take = 0, [FromQuery] int skip = 0, [FromQuery] LeaderboardFilter currentPeriod = LeaderboardFilter.Forever)
    {
        return Ok(await Mediator.Send(new GetLeaderboardPaginatedListQuery()
        {
            Take = take,
            Skip = skip,
            CurrentPeriod = currentPeriod
        }));
    }

    [HttpGet]
    public async Task<ActionResult<MobileLeaderboardViewModel>> GetMobilePaginated([FromQuery] int page = 0, [FromQuery] int pageSize = 10, [FromQuery] LeaderboardFilter currentPeriod = LeaderboardFilter.ThisWeek)
    {
        return Ok(await Mediator.Send(new GetMobileLeaderboardQuery
        {
            Page = page,
            PageSize = pageSize,
            CurrentPeriod = currentPeriod
        }));
    }

    [HttpGet]
    public async Task<ActionResult<EligibleUsersViewModel>> GetEligibleUsers(
        int? achievementId = null,
        bool filterStaff = false, 
        int top = 0,
        DateTime? dateFrom = null,
        DateTime? dateTo = null)
    {
        var request = new GetEligibleUsers
        {
            AchievementId = achievementId,
            FilterStaff = filterStaff,
            Top = top,
            DateFrom = dateFrom,
            DateTo = dateTo
        };

        var result = await Mediator.Send(request);
        return Ok(result);
    }
}