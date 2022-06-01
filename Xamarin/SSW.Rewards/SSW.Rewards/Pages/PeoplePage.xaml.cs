using SSW.Rewards.ViewModels;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class PeoplePage : ContentPage
    {
        private readonly DevProfilesViewModel _viewModel;

        public PeoplePage()
        {
            _viewModel = Resolver.Resolve<DevProfilesViewModel>();
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            await _viewModel.Initialise();

            Console.WriteLine("Finished initialising");
        }
    }
}