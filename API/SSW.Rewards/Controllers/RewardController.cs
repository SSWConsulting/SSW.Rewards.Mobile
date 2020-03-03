using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Reward.Commands;
using SSW.Rewards.Application.Reward.Queries.GetRewardAdminList;
using SSW.Rewards.Application.Reward.Queries.GetRewardList;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
	public class RewardController : BaseController
    {
	    [HttpGet]
        public async Task<ActionResult<RewardListViewModel>> List()
        {
            return Ok(await Mediator.Send(new GetRewardListQuery()));
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<RewardAdminListViewModel>> AdminList()
        {
            return Ok(await Mediator.Send(new GetRewardAdminListQuery()));
        }

        [HttpPost]
        public async Task<ActionResult<ClaimRewardResult>> Add([FromQuery] string rewardCode)
        {
            return Ok(await Mediator.Send(new ClaimRewardCommand { Code = rewardCode }));
        }
    }
}
