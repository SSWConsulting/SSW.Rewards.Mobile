using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Users.Commands.RegisterUser;
using SSW.Rewards.Application.Users.Commands.UploadProfilePic;
using SSW.Rewards.Application.Users.Queries.GetCurrentUser;
using SSW.Rewards.Application.Users.Queries.GetCurrentUserRoles;
using SSW.Rewards.Application.Users.Queries.GetUser;
using SSW.Rewards.Application.Users.Queries.GetUserAchievements;
using SSW.Rewards.Application.Users.Queries.GetUserRewards;
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

        [HttpGet("{id}")]
        public async Task<ActionResult<UserViewModel>> GetUser(int id)
        {
            return Ok(await Mediator.Send(new GetUserQuery() { Id = id }));
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

        [HttpGet]
        public async Task<ActionResult<string[]>> MyRoles()
        {
            return Ok(await Mediator.Send(new GetCurrentUserRolesQuery()));
        }

        [HttpPost]
        public async Task<ActionResult> Register()
        {
            return Ok(await Mediator.Send(new RegisterUserCommand()));
        }
    }
}