using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Rewards.PopupPages
{
    public partial class TechQuiz : PopupPage
    {
        private IUserService _userService { get; set;
        }
        public TechQuiz()
        {
            InitializeComponent();
            _userService = Resolver.Resolve<IUserService>();
        }

        private async void FindoutMore_Tapped(object sender, EventArgs e)
        {
            string quizUri = Constants.ApiBaseUrl + "/api/achievement/techquiz?user=" + await _userService.GetMyEmailAsync();

            await Browser.OpenAsync(quizUri, BrowserLaunchMode.External);
        }

        public async void Handle_CloseTapped(object sender, EventArgs args)
        {
            //            DisplayAlert("Close Tapped", "Close", "OK");
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}
