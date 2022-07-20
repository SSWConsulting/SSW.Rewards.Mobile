using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class QuizPage : ContentPage
    {
        private readonly QuizViewModel _viewModel;

        public QuizPage()
        {
            InitializeComponent();
            _viewModel = Resolver.Resolve<QuizViewModel>();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.Initialise();
        }
    }
}