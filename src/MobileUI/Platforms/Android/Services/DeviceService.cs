
using Android.Provider;

namespace SSW.Rewards.Mobile.Services;

public static partial class DeviceService
{
    public static partial string GetDeviceId()
    {
        var context = Android.App.Application.Context;
        var deviceId = Android.Provider.Settings.Secure.GetString(context.ContentResolver, Settings.Secure.AndroidId);
        return deviceId;
    }
}