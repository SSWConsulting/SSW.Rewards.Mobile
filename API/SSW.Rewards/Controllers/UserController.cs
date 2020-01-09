using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using SSW.Rewards.Application.User.Queries.GetCurrentUser;
using SSW.Rewards.Application.User.Queries.GetUserAchievements;
using SSW.Rewards.Application.User.Queries.GetUserRewards;
using System.Security.Claims;
using System.Linq;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using SSW.Rewards.Infrastructure;

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

        private static string FindFirstAndReplace(IEnumerable<object> e, string search)
        {
            return e.First(c => c.ToString().Contains(search)).ToString().Replace(search, string.Empty);
        }

        [HttpPost]
        public async Task<ActionResult<string>> UploadAvatar(IFormFile file)
        {
            return Ok(await Mediator.Send(new UploadAvatarQuery { File = file }));
        }
    }
}