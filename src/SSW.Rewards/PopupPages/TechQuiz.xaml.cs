﻿using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.Services;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

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
            string quizUri = App.Constants.ApiBaseUrl + "/api/achievement/techquiz?user=" + _userService.MyEmail;

            await Browser.OpenAsync(quizUri, BrowserLaunchMode.External);
        }

        public async void Handle_CloseTapped(object sender, EventArgs args)
        {
            //            DisplayAlert("Close Tapped", "Close", "OK");
            await PopupNavigation.Instance.PopAllAsync();
        }
    }
}