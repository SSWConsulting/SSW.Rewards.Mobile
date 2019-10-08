using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Consulting.PopupPages
{
    public partial class AboutSSW : PopupPage
    {
        public AboutSSW()
        {
            InitializeComponent();
        }

        private async void TapGestureRecognizer_Tapped(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://www.ssw.com.au/ssw/Company/AboutUs.aspx", BrowserLaunchMode.SystemPreferred);
        }
    }
}
