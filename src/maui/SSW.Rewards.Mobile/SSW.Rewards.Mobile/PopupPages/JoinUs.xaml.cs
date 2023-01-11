﻿using Mopups.Pages;
using Mopups.Services;

namespace SSW.Rewards.PopupPages
{
    public partial class JoinUs : PopupPage
    {
        public JoinUs()
        {
            InitializeComponent();
        }

        private async void FindoutMore_Tapped(object sender, EventArgs e)
        {
            await Browser.OpenAsync("https://www.ssw.com.au/ssw/Employment/default.aspx", BrowserLaunchMode.External);
        }

        public async void Handle_CloseTapped(object sender, EventArgs args)
        {
            await MopupService.Instance.PopAllAsync();
        }
    }
}