using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;
using SSW.Rewards.Enums;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.WebAPI.Controllers;

public class ActivityFeedController : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ActivityFeedViewModel>> GetAllActivities()
    {
        return Ok(await Mediator.Send(new GetActivitiesQuery()
        {
            Take = 50,
            Skip = 0,
            Filter = ActivityFeedFilter.All
        }));
    }
    
    [HttpGet]
    public async Task<ActionResult<ActivityFeedViewModel>> GetFriendsActivities()
    {
        return Ok(await Mediator.Send(new GetActivitiesQuery()
        {
            Take = 50,
            Skip = 0,
            Filter = ActivityFeedFilter.Friends
        }));
    }
}