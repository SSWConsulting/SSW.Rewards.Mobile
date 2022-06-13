using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Skills.Commands.DeleteSkill;
using SSW.Rewards.Application.Skills.Commands.UpsertSkill;
using SSW.Rewards.Application.Skills.Queries;
using SSW.Rewards.Application.Skills.Queries.GetAdminSkillList;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
    public class SkillController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<SkillListViewModel>> Get()
        {
            return Ok(await Mediator.Send(new GetSkillListQuery()));
        }

        [HttpGet]
        public async Task<ActionResult<List<AdminSkill>>> GetAdmin()
        {
            return Ok(await Mediator.Send(new GetAdminSkillListQuery()));
        }

        [HttpPut]
        public async Task<ActionResult> UpsertSkill(UpsertSkillCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpDelete]
        public async Task<ActionResult> DeleteSkill(DeleteSkillCommand command)
        {
            return Ok(await Mediator.Send(command));
        }


    }
}
