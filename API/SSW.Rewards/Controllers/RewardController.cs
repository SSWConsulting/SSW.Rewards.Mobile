using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Rewards.Commands;
using SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;
using SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;
using SSW.Rewards.Application.Rewards.Queries.Common;
using System.Threading.Tasks;
using SSW.Rewards.Application.Rewards.Commands.AddReward;
using SSW.Rewards.WebAPI.Settings;
using SSW.Rewards.Application.Rewards.Commands.DeleteReward;

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
        //[Authorize(Roles = AuthorizationRoles.Admin)]
        public async Task<ActionResult<RewardAdminListViewModel>> AdminList()
        {
            return Ok(await Mediator.Send(new GetRewardAdminListQuery()));
        }

        [HttpGet]
        //[Authorize(Roles = AuthorizationRoles.Admin)]
        public async Task<ActionResult<RecentRewardListViewModel>> GetRecent(GetRecentRewardsQuery query)
        {
            return Ok(await Mediator.Send(query));
        }

        [HttpPost]
        //[Authorize(Roles = AuthorizationRoles.Admin)]
        public async Task<ActionResult<int>> Add(AddRewardCommand addRewardCommand)
        {
            return Ok(await Mediator.Send(addRewardCommand));
        }

        [HttpPost]
        //[Authorize(Roles = AuthorizationRoles.Admin)]
        public async Task<ActionResult<ClaimRewardResult>> ClaimForUser(ClaimRewardForUserCommand claimRewardForUserCommand)
        {
            return Ok(await Mediator.Send(claimRewardForUserCommand));
        }

        [HttpPost]
        public async Task<ActionResult<ClaimRewardResult>> Claim([FromQuery] string rewardCode)
        {
            return Ok(await Mediator.Send(new ClaimRewardCommand { Code = rewardCode }));
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(DeleteRewardCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}
