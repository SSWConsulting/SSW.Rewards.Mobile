using System;
using System.Collections.Generic;
using SSW.Rewards.Services;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;

namespace SSW.Rewards.Views
{
    public partial class Profile : ContentPage
    {
        public Profile()
        {
            InitializeComponent();
            var viewModel = new ProfileViewModel(Resolver.Resolve<IUserService>());
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public Profile(ProfileViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public Profile(LeaderSummaryViewModel vm)
        {
            InitializeComponent();
            var viewModel = new ProfileViewModel(vm);
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
