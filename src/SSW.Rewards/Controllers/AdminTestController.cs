using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminTestController : ControllerBase
    {
        [HttpGet("Admin")]
        [Authorize(Roles ="admin")]
        public async Task<ActionResult> Admin()
        {
            return await Task.FromResult(Ok("admin only"));
        }

        [HttpGet("NotAdmin")]
        public async Task<ActionResult> NotAdmin()
        {
            return await Task.FromResult(Ok("non-admin permitted"));
        }
    }
}