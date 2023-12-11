using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Leaderboard.Queries.Common;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;
using SSW.Rewards.Application.PrizeDraw.Queries;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace SSW.Rewards.WebAPI.Controllers;

public class LeaderboardController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<LeaderboardListViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetLeaderboardListQuery()));
    }

    [HttpGet]
    public async Task<ActionResult<EligibleUsersViewModel>> GetEligibleUsers(int rewardId, LeaderboardFilter filter, bool balanceRequired, bool filterStaff)
    {
        return Ok(await Mediator.Send(new GetEligibleUsers() 
        {
            RewardId = rewardId, 
            Filter = filter,
            BalanceRequired = balanceRequired,
            FilterStaff = filterStaff
        }));
    }
}