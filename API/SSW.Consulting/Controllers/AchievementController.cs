using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.Achievement.Queries.GetAchievementList;
using SSW.Consulting.Application.User.Commands.UpsertUser;

namespace SSW.Consulting.WebAPI.Controllers
{
    public class AchievementController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<AchievementListViewModel>> List()
        {
            return Ok(await Mediator.Send(new GetAchievementListQuery()));
        }

        [HttpPost]
        public async Task<ActionResult<AchievementViewModel>> Add([FromQuery] string achievementCode)
        {
            return Ok(await Mediator.Send(new AddAchievementCommand { Code = achievementCode }));
        }
    }
}