using System;
using System.Collections.Generic;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using SSW.Rewards.Services;
using SSW.Rewards.PopupPages;
using Rg.Plugins.Popup.Services;


namespace SSW.Rewards.Controls
{
    public partial class FlyoutHeader : ContentView
    {
        public FlyoutHeader(FlyoutHeaderViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public async void Handle_ProfilePictureClicked(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PushAsync(new CameraPage());
        }

        public FlyoutHeader()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<FlyoutHeaderViewModel>();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
