using Android.App;
using Android.Content;
using Android.Content.PM;
using Xamarin.Essentials;

namespace SSW.Rewards.Droid
{
    [Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop)]
    [IntentFilter(new[] { Intent.ActionView },
    Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
    DataScheme = "msauth.com.ssw.consulting",
    DataHost = "auth")]
    public class WebCallbackActivity : WebAuthenticatorCallbackActivity
    {

    }

}
