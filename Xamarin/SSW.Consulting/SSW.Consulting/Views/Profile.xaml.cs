using System;
using System.Collections.Generic;
using SSW.Consulting.Services;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;

namespace SSW.Consulting.Views
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

        public Profile(MyProfileViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public Profile(LeaderSummaryViewModel vm)
        {
            InitializeComponent();
            var viewModel = new MyProfileViewModel(vm);
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
