using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Skills.Queries;
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
    }
}
