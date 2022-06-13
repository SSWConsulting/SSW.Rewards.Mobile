using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using SSW.Rewards.Helpers;
using SSW.Rewards.Models;
using SSW.Rewards.Services;
using SSW.Rewards.Pages;
using System;
using System.Threading.Tasks;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Rewards
{
    public partial class App : Application
    {
        public static Constants Constants = new Constants();

        public static object UIParent { get; set; }

        public App()
        {
            InitializeComponent();

            Resolver.Initialize();
            Resolver.Resolve<IPushNotificationActionService>()
                .ActionTriggered += NotificationActionTriggered;
            InitialiseApp();
        }

        private void InitialiseApp()
        {
            AppCenter.Start("android=" + Constants.AppCenterAndroidId + ";" +
                "ios=e33283b1-7326-447d-baae-e783ece0789b",
                typeof(Analytics), typeof(Crashes));


            MainPage = new LoginPage();
        }

        void NotificationActionTriggered(object sender, PushNotificationAction e) => ShowActionAlert(e);

        void ShowActionAlert(PushNotificationAction action) => MainThread.BeginInvokeOnMainThread(()
            => App.Current.MainPage?.DisplayAlert("App Test Push", $"{action} action received", "OK")
                .ContinueWith((task) => { if (task.IsFaulted) throw task.Exception; })
        );

        protected override async void OnStart()
        {
            //await UpdateAccessTokenAsync();
            await CheckApiCompatibilityAsync();
        }

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }

        private async Task CheckApiCompatibilityAsync()
        {
            try
            {
                ApiInfo info = new ApiInfo(Constants.ApiBaseUrl);

                bool compatible = await info.IsApiCompatibleAsync();

                if (!compatible)
                {
                    await Application.Current.MainPage.DisplayAlert("Update Required", "Looks like you're using an older version of the app. You can continue, but some features may not function as expected.", "OK");
                }
            }
            catch (Exception ex)
            {
                // TODO: log these instead to AppCenter
                Console.WriteLine("[App] ERROR checking API compat");
                Console.WriteLine($"[App] {ex.Message}");
                Console.WriteLine($"[App {ex.StackTrace}");
            }
        }
    }
}