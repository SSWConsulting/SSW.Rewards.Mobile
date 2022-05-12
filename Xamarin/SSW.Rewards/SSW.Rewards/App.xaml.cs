using System;
using Xamarin.Forms;
using SSW.Rewards.Views;
using Xamarin.Essentials;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;
using Microsoft.AppCenter.Push;
using SSW.Rewards.Helpers;
using System.Collections.Generic;
using System.Linq;

namespace SSW.Rewards
{
    public partial class App : Application
    {
        public static Constants Constants = new Constants();

        public static object UIParent { get; set; }

        public App()
        {
            InitializeComponent();
            InitialiseApp();
        }

        private void InitialiseApp()
        {
            Resolver.Initialize();

            AppCenter.Start("android=" + Constants.AppCenterAndroidId + ";" +
                "ios=e33283b1-7326-447d-baae-e783ece0789b",
                typeof(Analytics), typeof(Crashes), typeof(Push));


            if (Preferences.Get("FirstRun", true))
            {
                Preferences.Set("FirstRun", false);
                MainPage = new NavigationPage(new OnBoarding());
            }
            else
            {
                MainPage = new LoginPage();
            }
        }

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

        private async Task UpdateAccessTokenAsync()
        {
            bool loggedIn = Preferences.Get("LoggedIn", false);

            if (loggedIn)
            {
                try
                {
                    // TODO: move this to UserService

                    bool isStaff = false;

                    var qrCode = Preferences.Get("MyQRCode", string.Empty);

                    if (!string.IsNullOrWhiteSpace(qrCode))
                        isStaff = true;

                    Application.Current.MainPage = Resolver.ResolveShell(isStaff);
                    await Shell.Current.GoToAsync("//main");
                }
                catch (Exception e)
                {
                    Console.WriteLine("Service Unavailable");
                    Console.WriteLine(e);
                    //await Current.MainPage.DisplayAlert("Service Unavailable", "Looks like the SSW.Rewards service is not currently available. Please try again later.", "OK");
                }
            }
        }

        private async Task CheckApiCompatibilityAsync()
        {
            ApiInfo info = new ApiInfo(Constants.ApiBaseUrl);

            bool compatible = await info.IsApiCompatibleAsync();

            if (!compatible)
            {
                await Application.Current.MainPage.DisplayAlert("Update Required", "Looks like you're using an older version of the app. You can continue, but some features may not function as expected.", "OK");
            }
        }
    }
}