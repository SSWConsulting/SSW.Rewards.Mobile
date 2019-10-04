using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.Staff.Queries.GetStaffList;
using SSW.Consulting.WebAPI.Security;

namespace SSW.Consulting.WebAPI.Controllers
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