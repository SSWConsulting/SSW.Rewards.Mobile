using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Shared.DTOs.PrizeDraw;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;
using SSW.Rewards.Application.PrizeDraw.Queries;
using SSW.Rewards.Enums;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardPaginatedList;

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
            currentPeriod = currentPeriod
        }));
    }

    [HttpGet]
    public async Task<ActionResult<EligibleUsersViewModel>> GetEligibleUsers([FromQuery] int achievementId, LeaderboardFilter filter, bool filterStaff, int top)
    {
        var getEligibleUsers = new GetEligibleUsers
        {
            AchievementId = achievementId,
            Filter = filter,
            FilterStaff = filterStaff,
            Top = top
        };
        return Ok(await Mediator.Send(getEligibleUsers));
    }
}