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

namespace SSW.Consulting
{
    public partial class App : Application
    {

        public App()
        {
            InitializeComponent();

            DependencyService.Register<MockDataStore>();
            //MainPage = new AppShell();
            if(Preferences.Get("FirstRun", true))
            {
                Preferences.Set("FirstRun", false);
                MainPage = new NavigationPage(new OnBoarding());
            }
            else
            {
                if (Preferences.Get("LoggedIn", false))
                {
                    MainPage = Resolver.Resolve<AppShell>();
                }
                else
                {
                    MainPage = new LoginPage();
                }
            }
        }

        protected override void OnStart()
        {
            AppCenter.Start("android=60b96e0a-c6dd-4320-855f-ed58e44ffd00;" +
				  "ios=e33283b1-7326-447d-baae-e783ece0789b",
				  typeof(Auth), typeof(Analytics), typeof(Crashes));
		}

        protected override void OnSleep()
        {
            // Handle when your app sleeps
        }

        protected override void OnResume()
        {
            // Handle when your app resumes
        }
    }
}
