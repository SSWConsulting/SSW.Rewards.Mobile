namespace SSW.Rewards.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class QuizPage : ContentPage
{
    private readonly QuizViewModel _viewModel;

    public QuizPage(QuizViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Initialise();
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