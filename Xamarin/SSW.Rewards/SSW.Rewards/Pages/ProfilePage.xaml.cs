using SSW.Rewards.Services;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class ProfilePage : ContentPage
    {
        private bool _isMe;

        private ProfileViewModel viewModel;

        public ProfilePage()
        {
            InitializeComponent();
            viewModel = new ProfileViewModel(Resolver.Resolve<IUserService>());
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            _isMe = true;
        }

        public ProfilePage(LeaderSummaryViewModel vm)
        {
            InitializeComponent();
            viewModel = new ProfileViewModel(vm);
            viewModel.Navigation = Navigation;
            BindingContext = viewModel;

            _isMe = false;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await viewModel.Initialise(_isMe);
            await System.Threading.Tasks.Task.Delay(1000);
            TestSnackbar.ShowSnackbar();
        }
    }
}