using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.WebAPI.Authorisation;
using SSW.Rewards.Application.Roles.Commands.DeleteUserRole;
using SSW.Rewards.Application.Roles.Commands.InsertUserRole;

namespace SSW.Rewards.WebAPI.Controllers;

public class RolesController : ApiControllerBase
{
    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<int>> CreateRole(InsertUserRoleCommand insertUserRoleCommand)
    {
        return Ok(await Mediator.Send(insertUserRoleCommand));
    }    
    
    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> DeleteRole(DeleteUserRoleCommand deleteUserRoleCommand)
    {
        return Ok(await Mediator.Send(deleteUserRoleCommand));
    }
}