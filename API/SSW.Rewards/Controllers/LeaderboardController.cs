using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Leaderboard.Queries.GetLeaderboardList;

namespace SSW.Rewards.WebAPI.Controllers
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