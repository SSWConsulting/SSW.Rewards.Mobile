using SSW.Rewards.Controls;

namespace SSW.Rewards.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
[QueryProperty(nameof(QuizId), nameof(QuizId))]
[QueryProperty(nameof(QuizIcon), nameof(QuizIcon))]
public partial class QuizDetailsPage : ContentPage
{
    private QuizDetailsViewModel _viewModel;

    public string QuizId { get; set; }

    public string QuizIcon { get; set; }

    public QuizDetailsPage(QuizDetailsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        int quizId = int.Parse(QuizId);

        await _viewModel.Initialise(quizId, QuizIcon);
        _viewModel.OnNextQuestionRequested += ScrollToIndex;
        _viewModel.ShowSnackbar += ShowSnackbar;
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

    private async void ShowSnackbar(object sender, ShowSnackbarEventArgs e)
    {
        QuizResultsPageSnackbar.Options = e.Options;
        await QuizResultsPageSnackbar.ShowSnackbar();
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