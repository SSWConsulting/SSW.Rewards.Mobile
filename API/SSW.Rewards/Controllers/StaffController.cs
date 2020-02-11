using Microsoft.AspNetCore.Mvc;
using SSW.Rewards.Application.Staff.Queries.GetStaffList;
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
    }
}