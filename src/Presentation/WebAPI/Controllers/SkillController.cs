using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Skills.Commands.DeleteSkill;
using SSW.Rewards.Application.Skills.Commands.UpsertSkill;
using SSW.Rewards.Application.Skills.Queries;
using SSW.Rewards.Application.Skills.Queries.GetAdminSkillList;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class SkillController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SkillListViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetSkillListQuery()));
    }

    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<List<AdminSkill>>> GetAdmin()
    {
        return Ok(await Mediator.Send(new GetAdminSkillListQuery()));
    }

    [HttpPut]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> UpsertSkill(UpsertSkillCommand command)
    {
        return Ok(await Mediator.Send(command));
    }

    [HttpDelete]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> DeleteSkill(DeleteSkillCommand command)
    {
        return Ok(await Mediator.Send(command));
    }


}
