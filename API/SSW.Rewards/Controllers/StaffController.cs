using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Staff.Commands.AddStaffMemberProfile;
using SSW.Rewards.Application.Staff.Commands.DeleteStaffMemberProfile;
using SSW.Rewards.Application.Staff.Commands.UpsertStaffMemberProfile;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Application.Staff.Queries.GetStaffMemberProfile;
using SSW.Rewards.Domain.Entities;
using System.Threading.Tasks;

namespace SSW.Rewards.WebAPI.Controllers
{
    public class StaffController : BaseController
    {
        [HttpGet]
        public async Task<ActionResult<StaffListViewModel>> Get()
        {
            return Ok(await Mediator.Send(new GetStaffListQuery()));
        }

        [HttpGet]
        public async Task<ActionResult<StaffDto>> GetStaffMemberProfile(string name)
        {
            return Ok(await Mediator.Send(new GetStaffMemberProfileQuery() { Name = name }));
        }

        [HttpPost]
        public async Task<ActionResult<StaffMember>> AddStaffMemberProfile(AddStaffMemberProfileCommand staffMember)
        {
            return Ok(await Mediator.Send(staffMember));
        }

        [HttpPost]
        //[Authorize(Roles = "admin")]
        public async Task<ActionResult<string>> UpsertStaffMemberProfile(UpsertStaffMemberProfileCommand staffMember)
        {
            return Ok(await Mediator.Send(staffMember));
        }

        [HttpDelete]
        public async Task<ActionResult<string>> DeleteStaffMemberProfile(DeleteStaffMemberProfileCommand staffMember)
        {
            return Ok(await Mediator.Send(staffMember));
        }
    }
}