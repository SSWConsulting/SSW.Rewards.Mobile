using System;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;

namespace SSW.Rewards.Views
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
            try
            {
                Console.WriteLine("Initialising Login Page");

                InitializeComponent();

                Console.WriteLine("Trying to resolve login view model...");
                _viewModel = Resolver.Resolve<LoginPageViewModel>();

                Console.WriteLine("Login viewmodel resolved");
                _viewModel.Navigation = Navigation;
                BindingContext = _viewModel;
            }
            catch (Exception ex)
            {
                Console.WriteLine("I made a booboo mommy");
                Console.WriteLine(ex.Message);
                Console.WriteLine(ex.StackTrace);
                throw;
            }
        }

        //protected override async void OnAppearing()
        //{
        //    base.OnAppearing();

        //    await _viewModel.Refresh();
        //}
    }
}