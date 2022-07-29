using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Leaderboard.Queries.Common;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;

namespace SSW.Rewards.WebAPI.Controllers;

public class LeaderboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LeaderboardListViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetLeaderboardListQuery()));
    }
}