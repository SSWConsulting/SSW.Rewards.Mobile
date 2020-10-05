using System;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;


namespace SSW.Rewards.Views
{
    public partial class RewardsPage : ContentPage
    {
        public RewardsViewModel ViewModel { get; set; }
        public RewardsPage()
        {
            ViewModel = Resolver.Resolve<RewardsViewModel>();
            ViewModel.Navigation = Navigation;
            BindingContext = ViewModel;
            InitializeComponent();
        }

        public RewardsPage(RewardsViewModel viewModel)
        {
            ViewModel = viewModel;
            ViewModel.Navigation = Navigation;
            BindingContext = ViewModel;
            InitializeComponent();
        }
    }
}