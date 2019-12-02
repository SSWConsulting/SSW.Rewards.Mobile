using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.Common.Interfaces;
using SSW.Consulting.Application.Leaderboard.Queries.GetLeaderboardList;
using System.IdentityModel.Tokens.Jwt;
using SSW.Consulting.Application.Common.Helpers;

namespace SSW.Consulting.WebAPI.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class AdminController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<bool>> Get()
        {
            return AdminAuth.RequestFromAdmin(HttpContext.Request) ? Ok(await Task.FromResult(true)) : new UnauthorizedResult() as ActionResult;
        }
    }
}