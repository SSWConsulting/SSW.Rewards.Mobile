using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Consulting.PopupPages
{
    public partial class TechQuiz : PopupPage
    {
        public TechQuiz()
        {
            InitializeComponent();
        }

        private async void FindoutMore_Tapped(object sender, EventArgs e)
        {
            string url = Constants.ApiBaseUrl + "/TechQuiz";

            await Browser.OpenAsync(url, BrowserLaunchMode.SystemPreferred);
        }
    }
}
