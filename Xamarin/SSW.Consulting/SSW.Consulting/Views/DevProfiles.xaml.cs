using System;
using System.Collections.Generic;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Essentials;
using System.Threading.Tasks;

namespace SSW.Rewards.Views
{
    public partial class DevProfiles : ContentPage
    {
        private double _originalY { get; set; }

        public DevProfiles(DevProfilesViewModel viewModel)
        {
            InitializeComponent();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
            Initialise();
        }

        public DevProfiles()
        {
            InitializeComponent();
            var viewModel = Resolver.Resolve<DevProfilesViewModel>();
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;
            Initialise();
        }

        private void Initialise()
        {
            MessagingCenter.Subscribe<object>(this, "SlideUp", (obj) => { SlideUp(); });
            MessagingCenter.Subscribe<object>(this, "SlideDown", (obj) => { SlideDown(); });
        }

        void HandleSwiped(object sender, SwipedEventArgs e)
        {
            switch (e.Direction)
            {
                case SwipeDirection.Up:
                    SlideUp();
                    break;
                case SwipeDirection.Down:
                    SlideDown();
                    break;
            }
        }

        public void SlideUp()
        {
            MessagingCenter.Send<object>(this, "SlideOut");
            _originalY = bottomSheet.Y;
            imgScroll.RotateTo(0);
            bottomSheet.TranslateTo(bottomSheet.X, -450, 300, Easing.SinIn);
        }

        public void SlideDown()
        {
            MessagingCenter.Send<object>(this, "SlideIn");
            bottomSheet.TranslateTo(bottomSheet.X, 0, 300, Easing.SinIn);
            imgScroll.RotateTo(180);
        }
    }
}
