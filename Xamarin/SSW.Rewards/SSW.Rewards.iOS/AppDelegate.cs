using Foundation;
using Lottie.Forms.iOS.Renderers;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SSW.Rewards.iOS.Extensions;
using SSW.Rewards.iOS.Services;
using SSW.Rewards.Services;
using System;
using System.Diagnostics;
using System.Threading.Tasks;
using UIKit;
using UserNotifications;
using Xamarin.Essentials;

namespace SSW.Rewards.iOS
{
    // The UIApplicationDelegate for the application. This class is responsible for launching the 
    // User Interface of the application, as well as listening (and optionally responding) to 
    // application events from iOS.
    [Register("AppDelegate")]
    public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
    {
        IPushNotificationActionService _notificationActionService;
        INotificationRegistrationService _notificationRegistrationService;
        IDeviceInstallationService _deviceInstallationService;

        IPushNotificationActionService NotificationActionService
            => _notificationActionService ??
                (_notificationActionService =
                Resolver.Resolve<IPushNotificationActionService>());

        INotificationRegistrationService NotificationRegistrationService
            => _notificationRegistrationService ??
                (_notificationRegistrationService =
                Resolver.Resolve<INotificationRegistrationService>());

        IDeviceInstallationService DeviceInstallationService
            => _deviceInstallationService ??
                (_deviceInstallationService =
                Resolver.Resolve<IDeviceInstallationService>());

        //
        // This method is invoked when the application has loaded and is ready to run. In this 
        // method you should instantiate the window, load the UI into it and then make the window
        // visible.
        //
        // You have 17 seconds to return from this method, or iOS will terminate your application.
        //
        public override bool FinishedLaunching(UIApplication app, NSDictionary options)
        {
            global::Xamarin.Forms.Forms.SetFlags("Shell_Experimental", "Visual_Experimental", "CollectionView_Experimental", "FastRenderers_Experimental", "Brush_Experimental");

            Rg.Plugins.Popup.Popup.Init();

            global::Xamarin.Forms.Forms.Init();
            Resolver.InitializeNativeInstallation(new DeviceInstallationService());
            // Conditionally request authorization and register for remote notifications immediately after Bootstrap.
            if (DeviceInstallationService.NotificationsSupported)
            {
                UNUserNotificationCenter.Current.RequestAuthorization(
                    UNAuthorizationOptions.Alert |
                    UNAuthorizationOptions.Badge |
                    UNAuthorizationOptions.Sound,
                    (approvalGranted, error) =>
                    {
                        if (approvalGranted && error == null)
                            RegisterForRemoteNotifications();
                    }
                );
            }

            AnimationViewRenderer.Init();
            ZXing.Net.Mobile.Forms.iOS.Platform.Init();
            FFImageLoading.Forms.Platform.CachedImageRenderer.Init();

            ObjCRuntime.Class.ThrowOnInitFailure = false;

            AppCenter.Start("e33283b1-7326-447d-baae-e783ece0789b",
                   typeof(Analytics), typeof(Crashes));

            LoadApplication(new App());

            // If the options argument contains the UIApplication.LaunchOptionsRemoteNotificationKey, pass in the resulting userInfo object.
            using (var userInfo = options?.ObjectForKey(UIApplication.LaunchOptionsRemoteNotificationKey) as NSDictionary)
                ProcessNotificationActions(userInfo);

            return base.FinishedLaunching(app, options);
        }

        /// <summary>Register user notification settings and for remote notifications with APNS.</summary>
        void RegisterForRemoteNotifications()
        {
            MainThread.BeginInvokeOnMainThread(() =>
            {
                var pushSettings = UIUserNotificationSettings.GetSettingsForTypes(
                    UIUserNotificationType.Alert |
                    UIUserNotificationType.Badge |
                    UIUserNotificationType.Sound,
                    new NSSet());

                UIApplication.SharedApplication.RegisterUserNotificationSettings(pushSettings);
                UIApplication.SharedApplication.RegisterForRemoteNotifications();
            });
        }

        /// <summary>Set the <see cref="IDeviceInstallationService.Token"/> property value and refresh the registration.</summary>
        /// <remarks>
        /// Refresh the registration and cache the device token if it has been updated since it was last stored.
        /// </remarks>
        Task CompleteRegistrationAsync(NSData deviceToken)
        {
            DeviceInstallationService.Token = deviceToken.ToHexString();
            return NotificationRegistrationService.RefreshRegistrationAsync();
        }

        /// <summary>Processing the <see cref="NSDictionary"/> notification data and conditionally calls <see cref="PushNotificationActionService.TriggerAction"/>.</summary>
        void ProcessNotificationActions(NSDictionary userInfo)
        {
            if (userInfo == null)
                return;

            try
            {
                var actionValue = userInfo.ObjectForKey(new NSString("action")) as NSString;

                if (!string.IsNullOrWhiteSpace(actionValue?.Description))
                    NotificationActionService.TriggerAction(actionValue.Description);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
        /// <summary>Passing the <paramref name="deviceToken"/> argument to the <see cref="CompleteRegistrationAsync"/> method.</summary>
        public override void RegisteredForRemoteNotifications(UIApplication application, NSData deviceToken)
            => CompleteRegistrationAsync(deviceToken).ContinueWith((task)
                => { if (task.IsFaulted) throw task.Exception; });

        /// <summary>Passing the <paramref name="userInfo"/> argument to the <see cref="ProcessNotificationActions"/> method.</summary>
        public override void ReceivedRemoteNotification(UIApplication application, NSDictionary userInfo)
            => ProcessNotificationActions(userInfo);

        /// <summary>Logs error.</summary>
        /// TODO: implement proper logging and error handling for production scenarios.
        public override void FailedToRegisterForRemoteNotifications(UIApplication application, NSError error)
            => Debug.WriteLine(error.Description);
    }
}
