using System;
using System.Collections.Generic;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;

namespace SSW.Consulting.Views
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
