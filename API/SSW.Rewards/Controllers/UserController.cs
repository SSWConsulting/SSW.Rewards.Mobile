using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.User.Commands.UploadProfilePic;
using SSW.Rewards.Application.User.Queries.GetCurrentUser;
using SSW.Rewards.Application.User.Queries.GetUserAchievements;
using SSW.Rewards.Application.User.Queries.GetUserRewards;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
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

        [HttpGet]
        public async Task<ActionResult<UserRewardsViewModel>> Rewards([FromQuery] int userId)
        {
            return Ok(await Mediator.Send(new GetUserRewardsQuery { UserId = userId }));
        }

        [HttpPost]
        public async Task<ActionResult<string>> UploadProfilePic(IFormFile file)
        {
            return Ok(await Mediator.Send(new UploadProfilePicCommand { File = file }));
        }
    }
}