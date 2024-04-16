using Android.App;
using Firebase.Messaging;

namespace SSW.Rewards.Mobile.Platforms.Android;

[Service(Exported = false)]
[IntentFilter(["com.google.firebase.MESSAGING_EVENT"])]
public class RewardsFirebaseMessagingService : FirebaseMessagingService
{
    public override async void OnNewToken(string token)
    {
        base.OnNewToken(token);
        await SecureStorage.SetAsync("DeviceToken", token);
        Preferences.Set("DeviceTokenLastTimeUpdated", DateTime.MinValue);
    }
}