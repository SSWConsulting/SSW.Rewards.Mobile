using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
            return Ok("admin only");
        }

        [HttpGet("NotAdmin")]
        public async Task<ActionResult> NotAdmin()
        {
            //var user = HttpContext.User.Claims;
            return Ok("non-admin permitted");
        }
    }
}