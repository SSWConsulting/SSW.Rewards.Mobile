using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.System.Commands.SeedData;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
    public class SeedController : BaseController
    {
        [Authorize(Roles = "admin")]
        [HttpGet]
        public async Task<ActionResult> SeedData()
        {
            await Mediator.Send(new SeedSampleDataCommand());

            return Ok();
        }
    }
}
