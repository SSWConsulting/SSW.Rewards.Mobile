using System;
using SSW.Rewards.Services;

namespace SSW.Rewards
{
    public class Bootstrap
    {
        /// <summary>
        /// Called by each platform when the app launches passing in a platform-specific implementation of <see cref="IDeviceInstallationService"/>
        /// </summary>
        public static void Begin(Func<IDeviceInstallationService> deviceInstallationService)
        {
            ServiceContainer.Register(deviceInstallationService);

            ServiceContainer.Register<IPushNotificationActionService>(()
                => new PushNotificationActionService());

            ServiceContainer.Register<INotificationRegistrationService>(()
                => new NotificationRegistrationService());
        }
    }
}