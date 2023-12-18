using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Rewards.Commands;
using SSW.Rewards.Application.Rewards.Commands.AddReward;
using SSW.Rewards.Application.Rewards.Commands.DeleteReward;
using SSW.Rewards.Application.Rewards.Commands.UpdateReward;
using SSW.Rewards.Application.Rewards.Common;
using SSW.Rewards.Application.Rewards.Queries.Common;
using SSW.Rewards.Application.Rewards.Queries.GetOnboardingRewards;
using SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;
using SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;
using SSW.Rewards.Application.Rewards.Queries.SearchRewards;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class RewardController : ApiControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<RewardListViewModel>> GetOnboardingRewards()
    {
        return Ok(await Mediator.Send(new GetOnboardingRewards()));
    }

    [HttpGet]
    public async Task<ActionResult<RewardListViewModel>> List()
    {
        return Ok(await Mediator.Send(new GetRewardListQuery()));
    }

    [HttpGet]
    public async Task<ActionResult<RewardListViewModel>> Search([FromQuery] string searchTerm)
    {
        return Ok(await Mediator.Send(new SearchRewardsQuery { SearchTerm = searchTerm }));
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<RewardAdminListViewModel>> AdminList()
    {
        return Ok(await Mediator.Send(new GetRewardAdminListQuery()));
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<RecentRewardListViewModel>> GetRecent(GetRecentRewardsQuery query)
    {
        return Ok(await Mediator.Send(query));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<int>> Add(AddRewardCommand addRewardCommand)
    {
        return Ok(await Mediator.Send(addRewardCommand));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
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
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> Delete(DeleteRewardCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPatch]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> UpdateReward(UpdateRewardCommand command)
    {
        return Ok(await Mediator.Send(command));
    }
}
