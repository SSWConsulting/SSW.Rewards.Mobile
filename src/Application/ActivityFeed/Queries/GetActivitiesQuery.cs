using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace Microsoft.Extensions.DependencyInjection.ActivityFeed.Queries;

public class GetActivitiesQuery : IRequest<IList<ActivityFeedViewModel>>
{
    public ActivityFeedFilter Filter { get; set; }
    public int Skip { get; set; }
    public int Take { get; set; }
}

public class GetActivitiesQueryHandler : IRequestHandler<GetActivitiesQuery, IList<ActivityFeedViewModel>>
{
    private readonly IActivityFeedService _activityFeedService;

    public GetActivitiesQueryHandler(IActivityFeedService activityFeedService)
    {
        _activityFeedService = activityFeedService;
    }

    public async Task<IList<ActivityFeedViewModel>> Handle(GetActivitiesQuery request, CancellationToken cancellationToken)
    {
        return await _activityFeedService.GetActivities(request.Filter, request.Skip, request.Take, cancellationToken);
    }
}


