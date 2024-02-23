namespace SSW.Rewards.Mobile.Pages;

[QueryProperty(nameof(QuizId), nameof(QuizId))]
[QueryProperty(nameof(QuizIcon), nameof(QuizIcon))]
public partial class EarnDetailsPage
{
    private EarnDetailsViewModel _viewModel;

    public string QuizId { get; set; }

    public string QuizIcon { get; set; }

    public EarnDetailsPage(EarnDetailsViewModel viewModel)
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
    }

    protected override bool OnBackButtonPressed()
    {
        _viewModel.GoBack();
        return true;
    }

    private void InputView_OnTextChanged(object sender, TextChangedEventArgs e)
    {
        _viewModel.AnswerChangedCommand.Execute(e.NewTextValue);
    }
}