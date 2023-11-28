using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.System.Commands.SeedData;
using SSW.Rewards.Application.System.Commands.SeedV2Data;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class SeedController : ApiControllerBase
{
    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult> SeedData()
    {
        await Mediator.Send(new SeedSampleDataCommand());

        return Ok();
    }

    [Authorize(Roles = AuthorizationRoles.Admin)]
    [HttpGet]
    public async Task<ActionResult> SeedV2Data()
    {
        await Mediator.Send(new SeedV2DataCommand());

        return Ok();
    }
}
