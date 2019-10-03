using System;
using System.Collections.Generic;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;

namespace SSW.Consulting.Views
{
    public partial class OnBoarding : ContentPage
    {
        public OnBoarding(OnBoardingViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public OnBoarding()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<OnBoardingViewModel>();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
