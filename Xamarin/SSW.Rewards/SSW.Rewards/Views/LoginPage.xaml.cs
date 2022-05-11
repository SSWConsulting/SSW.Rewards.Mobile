using SSW.Rewards.ViewModels;
using Xamarin.Forms;

namespace SSW.Rewards.Views
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginPageViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public LoginPage()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<LoginPageViewModel>();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
