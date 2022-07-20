using SSW.Rewards.Services;
using SSW.Rewards.ViewModels;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SSW.Rewards.Pages
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    [QueryProperty(nameof(QuizId), nameof(QuizId))]
    public partial class QuizDetailsPage : ContentPage
    {
        private QuizDetailsViewModel _viewModel;

        public string QuizId { get; set; }

        public QuizDetailsPage()
        {
            InitializeComponent();
            _viewModel = new QuizDetailsViewModel(Resolver.Resolve<IQuizService>());
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            int quizId = int.Parse(QuizId);

            await _viewModel.Initialise(quizId);
        }
    }
}