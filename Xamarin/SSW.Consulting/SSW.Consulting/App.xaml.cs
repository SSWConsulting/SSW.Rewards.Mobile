using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using SSW.Consulting.Services;
using SSW.Consulting.Views;
using Xamarin.Essentials;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Auth;
using Microsoft.AppCenter.Crashes;
using System.Threading.Tasks;

namespace SSW.Consulting
{
    public partial class App : Application
    {
        public App()
        {
            AppCenter.Start("android=60b96e0a-c6dd-4320-855f-ed58e44ffd00;" +
				  "ios=e33283b1-7326-447d-baae-e783ece0789b",
				  typeof(Auth), typeof(Analytics), typeof(Crashes));

            InitializeComponent();

            DependencyService.Register<MockDataStore>();
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

        protected override void OnStart()
        {
            UpdateAccessTokenAsync();
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
                    //await Auth.SetEnabledAsync(true);
                    UserInformation userInfo = await Auth.SignInAsync();
                    string token = userInfo.AccessToken;
                    await SecureStorage.SetAsync("auth_token", token);

                    Application.Current.MainPage = Resolver.Resolve<AppShell>();
                    await Shell.Current.GoToAsync("//main");
                }
                catch(Exception e)
                {
                    await Current.MainPage.DisplayAlert("Service Unavailable", "Looks like the SSW.Consulting service is not currently available. Please try again later.", "OK");
                }
            }
        }
    }
}
