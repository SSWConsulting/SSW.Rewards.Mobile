using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Achievements.Command.ClaimAchievementForUser;
using SSW.Rewards.Application.Achievements.Command.DeleteAchievement;
using SSW.Rewards.Application.Achievements.Command.PostAchievement;
using SSW.Rewards.Application.Achievements.Queries.Common;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementAdminList;
using SSW.Rewards.Application.Achievements.Queries.GetAchievementList;
using SSW.Rewards.WebAPI.Settings;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
    public class AchievementController : BaseController
    {
        private readonly IWWWRedirectSettings _redirectSettings;

        public AchievementController(IWWWRedirectSettings redirectSettings)
        {
            _redirectSettings = redirectSettings;
        }

        [HttpGet]
        public async Task<ActionResult<AchievementListViewModel>> List()
        {
            return Ok(await Mediator.Send(new GetAchievementListQuery()));
        }

        [HttpGet]
        [Authorize(Roles = AuthorizationRoles.Admin)]
        public async Task<ActionResult<AchievementAdminListViewModel>> AdminList()
        {
            return Ok(await Mediator.Send(new GetAchievementAdminListQuery()));
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

        [AllowAnonymous]
        [HttpGet]
        public ActionResult TechQuiz([FromQuery] string user)
        {
            string url = string.Concat(_redirectSettings.TechQuizUrl, user);
            return Redirect(url);
        }

        [HttpDelete]
        public async Task<ActionResult> Delete(DeleteAchievementCommand command)
        {
            return Ok(await Mediator.Send(command));
        }
    }
}