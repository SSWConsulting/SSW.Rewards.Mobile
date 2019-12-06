using System;
using System.Collections.Generic;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;

namespace SSW.Rewards.Views
{
    public partial class EarnPointsPage : ContentPage
    {
        public EarnPointsPage(EarnPointsViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public EarnPointsPage()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<EarnPointsViewModel>();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
