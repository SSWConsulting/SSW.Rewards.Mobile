using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SSW.Rewards.WebAPI.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AdminTestController : ApiControllerBase
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