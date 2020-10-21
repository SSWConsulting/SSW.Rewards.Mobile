using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Achievement.Command.ClaimAchievementForUser;
using SSW.Rewards.Application.Achievement.Command.PostAchievement;
using SSW.Rewards.Application.Achievement.Commands.AddAchievement;
using SSW.Rewards.Application.Achievement.Queries.GetAchievementAdminList;
using SSW.Rewards.Application.Achievement.Queries.GetAchievementList;
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
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AchievementAdminListViewModel>> AdminList()
        {
            return Ok(await Mediator.Send(new GetAchievementAdminListQuery()));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<AchievementAdminViewModel>> Create([FromBody] CreateAchievementCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<ClaimAchievementResult>> ClaimForUser([FromBody] ClaimAchievementForUserCommand command)
        {
            return Ok(await Mediator.Send(command));
        }

        [HttpPost]
        public async Task<ActionResult<AchievementViewModel>> Add([FromQuery] string achievementCode)
        {
            return Ok(await Mediator.Send(new AddAchievementCommand { Code = achievementCode }));
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
    }
}