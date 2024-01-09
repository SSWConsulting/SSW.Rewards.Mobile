using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Shared.DTOs.Skills;
using SSW.Rewards.Application.Skills.Commands.DeleteSkill;
using SSW.Rewards.Application.Skills.Commands.UpsertSkill;
using SSW.Rewards.Application.Skills.Queries.GetSkillList;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class SkillController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<SkillsListViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetSkillList()));
    }


    [HttpPut]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<int>> UpsertSkill(SkillDto dto)
    {
        var command = new UpsertSkillCommand
        {
            Id = dto.Id,
            Skill = dto.Name
        };

        return Ok(await Mediator.Send(command));
    }

    [HttpDelete]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> DeleteSkill(int id)
    {
        var command = new DeleteSkillCommand
        {
            Id = id
        };

        return Ok(await Mediator.Send(command));
    }


}
