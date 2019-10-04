using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.User.Queries.GetUser;

namespace SSW.Consulting.WebAPI.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<UserViewModel>> Get()
        {
            return Ok(await Mediator.Send(new GetCurrentUserQuery()));
        }
    }
}