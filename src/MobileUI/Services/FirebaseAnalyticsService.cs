using Microsoft.Maui.Controls.Compatibility;
using Firebase.Analytics;

#if IOS
using Foundation;
#else
    using Android.Content;
    using Android.OS;
#endif

namespace SSW.Rewards.Mobile.Services;

public interface IFirebaseAnalyticsService
{
    void Log(string value, string eventName = "screen_view", string paramName = "screen_name");
    void Log(string eventName, IDictionary<string, string> parameters);
}

public class FirebaseAnalyticsService : IFirebaseAnalyticsService
{
    public void Log(string value, string eventName = "screen_view", string paramName = "screen_name")
    {
        Log(eventName, new Dictionary<string, string>
        {
            { paramName, value }
        });
    }

    public void Log(string eventName, IDictionary<string, string> parameters)
    {
#if IOS
        if (parameters == null)
        {
            Analytics.LogEvent(eventName, (Dictionary<object, object>)null);
            return;
        }

        var keys = new List<NSString>();
        var values = new List<NSString>();
        foreach (var item in parameters)
        {
            keys.Add(new NSString(item.Key));
            values.Add(new NSString(item.Value));
        }

        var parametersDictionary =
            NSDictionary<NSString, NSObject>.FromObjectsAndKeys(values.ToArray(), keys.ToArray(), keys.Count);
        Analytics.LogEvent(eventName, parametersDictionary);
#else
        var firebaseAnalytics = FirebaseAnalytics.GetInstance(Platform.CurrentActivity);

        if (parameters == null)
        {
            firebaseAnalytics.LogEvent(eventName, null);
            return;
        }

        var bundle = new Bundle();
        foreach (var param in parameters)
        {
            bundle.PutString(param.Key, param.Value);
        }

        firebaseAnalytics.LogEvent(eventName, bundle);
#endif
    }
}