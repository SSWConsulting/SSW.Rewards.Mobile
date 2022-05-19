using System;
using System.Collections.ObjectModel;
using SSW.Rewards.Models;
using SSW.Rewards.Services;
using UIKit;

namespace SSW.Rewards.iOS.Services
{
    /// <summary>Provides a unique ID using the <see cref="UIDevice.IdentifierForVendor"/> value and the notification hub registration payload.</summary>
    public class DeviceInstallationService : IDeviceInstallationService
    {
        const int SupportedVersionMajor = 13;
        const int SupportedVersionMinor = 0;

        public string Token { get; set; }

        public bool NotificationsSupported
            => UIDevice.CurrentDevice.CheckSystemVersion(SupportedVersionMajor, SupportedVersionMinor);

        public string GetDeviceId()
            => UIDevice.CurrentDevice.IdentifierForVendor.ToString();

        public DeviceInstall GetDeviceInstallation(params string[] tags)
        {
            if (!NotificationsSupported)
                throw new Exception(GetNotificationsSupportError());

            if (string.IsNullOrWhiteSpace(Token))
                throw new Exception("Unable to resolve token for APNS");

            var installation = new DeviceInstall
            {
                InstallationId = GetDeviceId(),
                Platform = "apns",
                PushChannel = Token
            };
            installation.Tags.AddRange(tags);

            return installation;
        }

        string GetNotificationsSupportError()
        {
            if (!NotificationsSupported)
                return $"This app only supports notifications on iOS {SupportedVersionMajor}.{SupportedVersionMinor} and above. You are running {UIDevice.CurrentDevice.SystemVersion}.";

            if (Token == null)
                return $"This app can support notifications but you must enable this in your settings.";

            return "An error occurred preventing the use of push notifications";
        }
    }
}