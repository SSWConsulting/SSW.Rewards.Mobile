using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Consulting.PopupPages
{
    public partial class AboutSSW : PopupPage
    {
        public async void Handle_CloseTapped(object sender, EventArgs args)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

        public AboutSSW()
        {
            InitializeComponent();
        }

        private async void FindoutMore_Tapped(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://www.ssw.com.au/ssw/Company/AboutUs.aspx", BrowserLaunchMode.SystemPreferred);
        }
    }
}
