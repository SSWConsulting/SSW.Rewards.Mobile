using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace SSW.Consulting.WebAPI.Controllers
{
    public class UserController : BaseController
    {
        [HttpGet]
        public string Get()
        {
            return User.Identity.Name;
        }

        //public async Task<ActionResult<UserViewModel>> Get()
        //{ 
        //    return Ok(await Mediator.Send(new GetActiveUserQuery()));
        //}

    }
}