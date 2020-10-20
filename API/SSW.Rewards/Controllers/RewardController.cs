using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Reward.Commands;
using SSW.Rewards.Application.Reward.Queries.GetRecentRewards;
using SSW.Rewards.Application.Reward.Queries.GetRewardAdminList;
using SSW.Rewards.Application.Reward.Queries.Common;
using System.Threading.Tasks;
using SSW.Rewards.Application.Reward.Commands.AddReward;

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

        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<RecentRewardListViewModel>> GetRecent(GetRecentRewardsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<int>> Add(AddRewardCommand addRewardCommand)
        {
            return Ok(await Mediator.Send(addRewardCommand));
        }

        [HttpPost]
        public async Task<ActionResult<ClaimRewardResult>> Claim([FromQuery] string rewardCode)
        {
            return Ok(await Mediator.Send(new ClaimRewardCommand { Code = rewardCode }));
        }
    }
}
