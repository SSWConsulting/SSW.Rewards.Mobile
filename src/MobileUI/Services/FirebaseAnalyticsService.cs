using Plugin.Firebase.Analytics;

namespace SSW.Rewards.Mobile.Services;

// From https://gist.github.com/vhugogarcia/abbdafa01d17c754eab747e06d2c7653#file-firebaseanalyticsservice-cs

public interface IFirebaseAnalyticsService
{
    void Log(string value, string eventName = "screen_view", string paramName = "screen_name");
    void Log(string eventName, IDictionary<string, object> parameters);
}

public class FirebaseAnalyticsService : IFirebaseAnalyticsService
{
    public void Log(string value, string eventName = "screen_view", string paramName = "screen_name")
    {
        Log(eventName, new Dictionary<string, object>
        {
            { paramName, value }
        });
    }

    public void Log(string eventName, IDictionary<string, object> parameters)
    {
        CrossFirebaseAnalytics.Current.LogEvent(eventName, parameters);
    }
}