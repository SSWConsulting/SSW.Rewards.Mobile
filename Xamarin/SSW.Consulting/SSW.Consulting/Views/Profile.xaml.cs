using System;
using System.Collections.Generic;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;

namespace SSW.Consulting.Views
{
    public partial class Profile : ContentPage
    {
        public Profile()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<ProfileViewModel>();
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
