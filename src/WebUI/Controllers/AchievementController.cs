using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Achievements.Command.ClaimAchievementForUser;
using SSW.Rewards.Application.Achievements.Command.DeleteAchievement;
using SSW.Rewards.Application.Achievements.Command.PostAchievement;
using SSW.Rewards.Application.Achievements.Command.UpdateAchievement;
using SSW.Rewards.Application.Achievements.Queries.Common;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementList;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class AchievementController : ApiControllerBase
{

    [HttpGet]
    public async Task<ActionResult<AchievementListViewModel>> List()
    {
        return Ok(await Mediator.Send(new GetAchievementListQuery()));
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<AchievementAdminListViewModel>> AdminList([FromQuery] bool includeArchived = false)
    {
        return Ok(await Mediator.Send(new GetAchievementAdminListQuery
        {
            IncludeArchived = includeArchived,
        }));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<AchievementAdminViewModel>> Create([FromBody] CreateAchievementCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<ClaimAchievementResult>> ClaimForUser([FromBody] ClaimAchievementForUserCommand command)
    {
        return Ok(await Mediator.Send(command));
    }


    [HttpPost]
    public async Task<ActionResult<PostAchievementResult>> Post([FromQuery] string achievementCode)
    {
        return Ok(await Mediator.Send(new PostAchievementCommand { Code = achievementCode }));
    }

    [HttpDelete]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> Delete(DeleteAchievementCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpPatch]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> UpdateAchievement(UpdateAchievementCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

}