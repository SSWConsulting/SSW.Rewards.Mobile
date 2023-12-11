using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Leaderboard.Queries.Common;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;
using SSW.Rewards.Application.PrizeDraw.Queries;

namespace SSW.Rewards.WebAPI.Controllers;

public class LeaderboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LeaderboardListViewModel>> Get()
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