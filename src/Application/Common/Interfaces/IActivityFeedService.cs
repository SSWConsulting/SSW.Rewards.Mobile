using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.Application.Common.Interfaces;

public interface IActivityFeedService
{
    public Task<IList<ActivityFeedViewModel>> GetActivities(ActivityFeedFilter filter, int skip, int take, CancellationToken cancellationToken);
}