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
            _viewModel.OnNextQuestionRequested += ScrollToIndex;
        }

        private void ScrollToIndex(object sender, int index)
        {
            QuestionsCarousel.ScrollTo(index);
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _viewModel.OnNextQuestionRequested -= ScrollToIndex;
        }

        protected override void OnSizeAllocated(double width, double height)
        {
            base.OnSizeAllocated(width, height);

            var titleWidth = TitleText.Width;

            var pageCenter = width / 2;

            var titleCenter = titleWidth / 2;

            var desiredStart = pageCenter - titleCenter;

            var titleX = TitleText.X;

            var desiredTranslation = titleX - desiredStart;

            TitleText.TranslationX = desiredTranslation;
        }
    }
}