using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.WebAPI.Controllers;

public class ActivityFeedController : ApiControllerBase
{
    [HttpGet]
    
    public async Task<ActionResult<IList<ActivityFeedViewModel>>> GetActivities([FromQuery] ActivityFeedFilter filter, int skip, int take)
    {
        return Ok(await Mediator.Send(new GetActivitiesQuery
        {
            Filter = filter,
            Skip = skip,
            Take = take
        }));
    }
}