using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.Mobile.Services;

public interface IActivityService
{
    Task<ActivityFeedViewModel> GetActivityFeed();

    Task<ActivityFeedViewModel> GetFriendsFeed();
}
