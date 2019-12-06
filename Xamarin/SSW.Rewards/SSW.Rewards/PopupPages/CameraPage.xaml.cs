using System;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using SSW.Rewards.ViewModels;


namespace SSW.Rewards.PopupPages
{
    public partial class CameraPage : PopupPage
    {
        public async void Handle_Tapped(object sender, EventArgs e)
        {
            await PopupNavigation.Instance.PopAllAsync();
        }

        private CameraPageViewModel _viewModel { get; set; }

        public CameraPage()
        {
            InitializeComponent();
            _viewModel = Resolver.Resolve<CameraPageViewModel>();
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
        }
    }
}
