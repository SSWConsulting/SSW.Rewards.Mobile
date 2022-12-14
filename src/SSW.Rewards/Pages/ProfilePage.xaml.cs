using SSW.Rewards.Controls;
using SSW.Rewards.Services;
using SSW.Rewards.ViewModels;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        private bool _isMe;

        private bool _initialised;

        private ProfileViewModel viewModel;

        public ProfilePage()
        {
            InitializeComponent();
            viewModel = new ProfileViewModel(Resolver.Resolve<IUserService>());
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            _isMe = true;
        }

        public ProfilePage(LeaderViewModel vm)
        {
            InitializeComponent();
            viewModel = new ProfileViewModel(vm);
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            _isMe = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            if (_isMe && _initialised)
            {

            }
            else
            {
                await viewModel.Initialise(_isMe);
            }
            viewModel.ShowSnackbar += ShowSnackbar;
            _initialised = true;
        }

        private async void ShowSnackbar(object sender, ShowSnackbarEventArgs e)
        {
            ProfilePageSnackbar.Options = e.Options;
            await ProfilePageSnackbar.ShowSnackbar();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            viewModel.OnDisappearing();
        }
    }
}