using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Network.Queries;
using SSW.Rewards.Shared.DTOs.Network;

namespace SSW.Rewards.WebAPI.Controllers;

public class NetworkController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<NetworkProfileListViewModel>> GetList()
    {
        return Ok(await Mediator.Send(new GetNetworkProfileListQuery()));
    }
}