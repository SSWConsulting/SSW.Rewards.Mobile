using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Shared.DTOs.Leaderboard;
using SSW.Rewards.Shared.DTOs.PrizeDraw;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;
using SSW.Rewards.Application.PrizeDraw.Queries;
using SSW.Rewards.Enums;

namespace SSW.Rewards.WebAPI.Controllers;

public class LeaderboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LeaderboardViewModel>> Get([FromQuery] int take = 0, [FromQuery] int skip = 0)
    {
        return Ok(await Mediator.Send(new GetLeaderboardListQuery() 
        {
            Take = take,
            Skip = skip
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