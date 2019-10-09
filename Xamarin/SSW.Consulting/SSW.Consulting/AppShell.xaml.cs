using System;
using System.Collections.Generic;
using Microsoft.AppCenter.Auth;
using Rg.Plugins.Popup.Services;
using SSW.Consulting.PopupPages;
using SSW.Consulting.Services;
using SSW.Consulting.Views;
using Xamarin.Forms;

namespace SSW.Consulting
{
    public partial class AppShell : Xamarin.Forms.Shell
    {
        private IUserService _userService { get; set; }

        public async void Handle_LogOutClicked(object sender, EventArgs e)
        {
            await _userService.SignOutAsync();
            await Navigation.PushModalAsync(new LoginPage());
        }

        public async void Handle_QuizClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new TechQuiz());
        }

        public async void Handle_EventsClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new Events());
        }

        public async void Handle_JoinClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new JoinUs());
        }

        public async void Handle_AboutClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new AboutSSW());
        }

        public void Handle_HowToPlayClicked(object sender, EventArgs e)
        {
            Navigation.PushModalAsync(new OnBoarding());
        }

        public AppShell(IUserService userService)
        {
            InitializeComponent();
            _userService = userService;
        }
    }
}
