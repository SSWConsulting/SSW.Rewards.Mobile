using System;
using System.Collections.Generic;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;

namespace SSW.Consulting.Views
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
