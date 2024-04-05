using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.Mobile.Services;

public interface IActivityService
{
    Task<IEnumerable<ActivityFeedViewModel>> GetActivityFeed();

    Task<IEnumerable<ActivityFeedViewModel>> GetFriendsFeed();
}
