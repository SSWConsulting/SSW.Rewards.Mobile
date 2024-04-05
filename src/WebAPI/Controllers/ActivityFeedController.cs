using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.WebAPI.Controllers;

public class ActivityFeedController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityFeedViewModel>>> GetAllActivities([FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        return Ok(await Mediator.Send(new GetActivitiesQuery
        {
            Take = take,
            Skip = skip,
            Filter = ActivityFeedFilter.All
        }));
    }
    
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ActivityFeedViewModel>>> GetFriendsActivities([FromQuery] int take = 50, [FromQuery] int skip = 0)
    {
        return Ok(await Mediator.Send(new GetActivitiesQuery
        {
            Take = take,
            Skip = skip,
            Filter = ActivityFeedFilter.Friends,
        }));
    }
}