using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Staff.Commands.DeleteStaffMemberProfile;
using SSW.Rewards.Application.Staff.Commands.UploadStaffMemberProfilePicture;
using SSW.Rewards.Application.Staff.Commands.UpsertStaffMemberProfile;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Application.Staff.Queries.GetStaffMemberProfile;
using SSW.Rewards.WebAPI.Authorisation;

namespace SSW.Rewards.WebAPI.Controllers;

public class StaffController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<StaffListViewModel>> Get()
    {
        return Ok(await Mediator.Send(new GetStaffListQuery()));
    }

    [HttpGet]
    public async Task<ActionResult<StaffDto>> GetStaffMemberProfile(int id)
    {
        return Ok(await Mediator.Send(new GetStaffMemberProfileQuery() { Id = id }));
    }

    [HttpGet]
    public async Task<ActionResult<StaffDto>> GetStaffMemberByEmail(string email)
    {
        return Ok(await Mediator.Send(new GetStaffMemberProfileQuery() { email = email, GetByEmail = true }));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<StaffDto>> UpsertStaffMemberProfile(UpsertStaffMemberProfileCommand staffMember)
    {
        return Ok(await Mediator.Send(staffMember));
    }

    [HttpPost]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult<string>> UploadStaffMemberProfilePicture(int id, IFormFile file)
    {
        return Ok(await Mediator.Send(new UploadStaffMemberProfilePictureCommand { Id = id, File = file }));
    }

    [HttpDelete]
    [Authorize(Roles = AuthorizationRoles.Admin)]
    public async Task<ActionResult> DeleteStaffMemberProfile(DeleteStaffMemberProfileCommand staffMember)
    {
        return Ok(await Mediator.Send(staffMember));
    }
}