using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Network.Queries;

namespace SSW.Rewards.WebAPI.Controllers;

public class NetworkController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NetworkProfileListViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetNetworkProfileListQuery()));
    }
}