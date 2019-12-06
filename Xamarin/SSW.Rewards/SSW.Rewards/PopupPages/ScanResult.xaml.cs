using System;
using System.Collections.Generic;
using Rg.Plugins.Popup.Pages;
using SSW.Rewards.Models;
using SSW.Rewards.Services;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;

namespace SSW.Rewards.PopupPages
{
    public partial class ScanResult : PopupPage
    {
        public ScanResult(string scanData)
        {
            InitializeComponent();
            ScanResultViewModel viewModel = new ScanResultViewModel(scanData, Resolver.Resolve<IUserService>());
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
