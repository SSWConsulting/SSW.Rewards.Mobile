using Microsoft.AspNetCore.Mvc;
using SSW.Consulting.Application.Reward.Commands;
using SSW.Consulting.Application.Reward.Queries.GetRewardList;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SSW.Consulting.WebAPI.Controllers
{
    public class RewardController : BaseController
    {

        public RewardController()
        {

        }

        [HttpGet]
        public async Task<ActionResult<RewardListViewModel>> List()
        {
            return Ok(await Mediator.Send(new GetRewardListQuery()));
        }

        [HttpPost]
        public async Task<ActionResult<ClaimRewardResult>> Add([FromQuery] string rewardCode)
        {
            return Ok(await Mediator.Send(new ClaimRewardCommand { Code = rewardCode }));
        }
    }
}
