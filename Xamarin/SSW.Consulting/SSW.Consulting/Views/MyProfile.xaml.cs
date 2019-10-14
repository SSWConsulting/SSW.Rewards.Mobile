using System;
using System.Collections.Generic;
using SSW.Consulting.ViewModels;
using Xamarin.Forms;

namespace SSW.Consulting.Views
{
    public partial class MyProfile : ContentPage
    {
        public MyProfile()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<MyProfileViewModel>();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public MyProfile(MyProfileViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }

        public MyProfile(LeaderSummaryViewModel vm)
        {
            InitializeComponent();
            var viewModel = new MyProfileViewModel(vm);
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
        }
    }
}
