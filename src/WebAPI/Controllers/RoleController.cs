using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Roles.Queries.GetRoles;
using SSW.Rewards.Shared.DTOs.Roles;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class RoleController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
    {
        return Ok(await Mediator.Send(new GetRolesQuery()));
    }
}