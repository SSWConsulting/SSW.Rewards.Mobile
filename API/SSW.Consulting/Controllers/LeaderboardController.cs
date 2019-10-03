using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList;

namespace SSW.Consulting.WebAPI.Controllers
{
    public class LeaderboardController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<LeaderboardListViewModel>> Get()
        {
            return Ok(await Mediator.Send(new GetLeaderboardListQuery()));
        }
    }
}