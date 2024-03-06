using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.AddressLookup;
using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.WebAPI.Controllers;

public class AddressController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Address>>> Get(string query)
    {
        return Ok(await Mediator.Send(new AddressLookupQuery() { QueryString = query }));
    }
}