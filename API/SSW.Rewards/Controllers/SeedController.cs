using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.System.Commands.SeedData;
using SSW.Rewards.WebAPI.Settings;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
    public class SeedController : BaseController
    {
        [Authorize(Roles = AuthorizationRoles.Admin)]
        [HttpGet]
        public async Task<ActionResult> SeedData()
        {
            await Mediator.Send(new SeedSampleDataCommand());

            return Ok();
        }
    }
}
