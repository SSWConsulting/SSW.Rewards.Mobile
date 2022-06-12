using SSW.Rewards.ViewModels;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace SSW.Rewards.Pages
{
    public partial class LoginPage : ContentPage
    {
        private readonly LoginPageViewModel _viewModel;

        public LoginPage(LoginPageViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
        }

        public LoginPage()
        {
            InitializeComponent();

            _viewModel = Resolver.Resolve<LoginPageViewModel>();

            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await Task.WhenAny<bool>
                (
                    LogoImage.TranslateTo(0, -200, 1000, Easing.CubicIn),
                    LogoImage.ScaleTo(3, 1000, Easing.CubicIn)

                );

            await _viewModel.Refresh();
        }
    }
}