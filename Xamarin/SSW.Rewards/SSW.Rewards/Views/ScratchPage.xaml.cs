using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using SSW.Rewards.ViewModels;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace SSW.Rewards.Views
{
    public partial class ScratchPage : ContentPage
    {
        private double _originalY { get; set; }

        public ScratchPage()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<DevProfilesViewModel>();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
            Initialise();
        }

        public ScratchPage(DevProfilesViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
            Initialise();
        }

        private void Initialise()
        {
            MessagingCenter.Subscribe<object>(this, "SlideUp", (obj) => { SlideUp(); });
            MessagingCenter.Subscribe<object>(this, "SlideDown", (obj) => { SlideDown(); });
        }

        public void SlideUp()
        {
            _originalY = bottomSheet.Y;
            imgScroll.RotateTo(0);
            bottomSheet.TranslateTo(bottomSheet.X, -400, 300, Easing.SinIn);
        }

        public void SlideDown()
        {
            bottomSheet.TranslateTo(bottomSheet.X, 0, 300, Easing.SinIn);
            imgScroll.RotateTo(180);
        }
    }
}
