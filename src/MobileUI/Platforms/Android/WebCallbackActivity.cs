using Android.App;
using Android.Content;
using Android.Content.PM;

namespace SSW.Rewards.Droid;

[Activity(NoHistory = true, LaunchMode = LaunchMode.SingleTop, Exported = true)]
[IntentFilter(new[] { Intent.ActionView },
Categories = new[] { Intent.CategoryDefault, Intent.CategoryBrowsable },
DataScheme = "msauth.com.ssw.consulting",
DataHost = "auth")]
public class WebCallbackActivity : WebAuthenticatorCallbackActivity
{
    public override void StartActivityForResult(Intent intent, int requestCode)
    {
        // Fix crash NullPointerException Attempt to invoke virtual method 'boolean android.content.Intent.migrateExtraStreamToClipData(android.content.Context)' on a null object reference
        // https://stackoverflow.com/questions/38041230/intent-migrateextrastreamtoclipdata-on-a-null-object-reference
        if (intent == null)
        {
            intent = new Intent();
        }

        base.StartActivityForResult(intent, requestCode);
    }
}
