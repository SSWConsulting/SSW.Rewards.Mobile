using System;
using SSW.Rewards.Controls;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PeoplePage : ContentPage
    {
        private readonly PeoplePageViewModel _viewModel;

        public PeoplePage()
        {
            _viewModel = Resolver.Resolve<PeoplePageViewModel>();
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            _viewModel.PageInView = true;
            _viewModel.ScrollToRequested += ScrollToIndex;
            _viewModel.ShowSnackbar += ShowSnackbar;
            await _viewModel.Initialise();
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.PageInView = false;
            _viewModel.ScrollToRequested -= ScrollToIndex;
            _viewModel.ShowSnackbar -= ShowSnackbar;
        }

        private void ScrollToIndex(object sender, ScrollToEventArgs e)
        {
            PicCarousel.ScrollTo(e.Index, -1, e.Position, e.Animate);
        }

        private async void ShowSnackbar(object sender, ShowSnackbarEventArgs e)
        {
            PeoplePageSnackbar.Options = e.Options;
            await PeoplePageSnackbar.ShowSnackbar();
        }
    }
}