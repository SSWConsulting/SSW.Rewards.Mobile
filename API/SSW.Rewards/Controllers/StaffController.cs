using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
using SSW.Rewards.Application.Staff.Queries.GetStaffMemberProfile;
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
    }
}