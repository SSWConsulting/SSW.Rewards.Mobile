using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.User.Queries.GetCurrentUser;
using SSW.Consulting.Application.User.Queries.GetUserAchievements;

namespace SSW.Consulting.WebAPI.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<CurrentUserViewModel>> Get()
        {
            return Ok(await Mediator.Send(new GetCurrentUserQuery()));
        }

        [HttpGet]
        public async Task<ActionResult<UserAchievementsViewModel>> Achievements([FromQuery] int userId)
        {
            return Ok(await Mediator.Send(new GetUserAchievementsQuery { UserId = userId }));
        }
    }
}