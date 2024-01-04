using Microsoft.AspNetCore.Mvc;
using Shared.DTOs.Leaderboard;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;
using SSW.Rewards.Application.PrizeDraw.Queries;
using SSW.Rewards.Enums;

namespace SSW.Rewards.WebAPI.Controllers;

public class LeaderboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LeaderboardViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetLeaderboardListQuery()));
    }

    [HttpGet]
    public async Task<ActionResult<EligibleUsersViewModel>> GetEligibleUsers([FromQuery] int achievementId, LeaderboardFilter filter, bool filterStaff)
    {
        var getEligibleUsers = new GetEligibleUsers
        {
            AchievementId = achievementId,
            Filter = filter,
            FilterStaff = filterStaff
        };
        return Ok(await Mediator.Send(getEligibleUsers));
    }
}