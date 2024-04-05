using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.ActivityFeed;

namespace SSW.Rewards.Mobile.Services;

public class ActivityService : IActivityService
{
    private readonly IActivityFeedService _activityClient;

    public ActivityService(IActivityFeedService activityService)
    {
        _activityClient = activityService;
    }
    
    public async Task<IEnumerable<ActivityFeedViewModel>> GetActivityFeed()
    {
        try
        {
            var vm = await _activityClient.GetAllActivities(CancellationToken.None);

            return vm;
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the activity feed. Please try again soon.", "OK");
            }

            return null;
        }
    }
    
    public async Task<IEnumerable<ActivityFeedViewModel>> GetFriendsFeed()
    {
        try
        {
            var vm = await _activityClient.GetFriendsActivities(CancellationToken.None);

            return vm;
        }
        catch (Exception e)
        {
            if (! await ExceptionHandler.HandleApiException(e))
            {
                await App.Current.MainPage.DisplayAlert("Oops...", "There seems to be a problem loading the activity feed. Please try again soon.", "OK");
            }

            return null;
        }
    }
}
