using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LeaderboardPage : ContentPage
    {
        private readonly LeaderBoardViewModel _viewModel;

        public LeaderboardPage()
        {
            _viewModel = Resolver.Resolve<LeaderBoardViewModel>();
            _viewModel.Navigation = Navigation;
            BindingContext = _viewModel;
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.Initialise();
        }
    }
}