using UIKit;

namespace SSW.Rewards.Mobile.Services;

public static partial class DeviceService
{
    public static partial string GetDeviceId()
    {
        var deviceId = UIDevice.CurrentDevice.IdentifierForVendor.ToString();
        return deviceId;
    }
}