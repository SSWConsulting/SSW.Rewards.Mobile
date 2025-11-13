namespace SSW.Rewards.Mobile.Pages;

public partial class QuizPage
{
    private readonly QuizViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    
    private bool _isLoaded;

    public QuizPage(QuizViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("QuizPage");
        await _viewModel.Initialise();
        await Animate();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
    
    private async Task Animate()
    {
        if (_isLoaded)
        {
            return;
        }
        
        CarouselSection.Opacity = 0;
        CarouselSection.TranslationY = -400;
        
        QuizListSection.Opacity = 0;
        QuizListSection.TranslationY = 400;
        
        await Task.Delay(100);
        
        await Task.WhenAll
        (
            CarouselSection.FadeTo(1, 600, Easing.CubicIn),
            QuizListSection.FadeTo(1, 600, Easing.CubicIn),
            CarouselSection.TranslateTo(0, 0, 600, Easing.SinIn),
            QuizListSection.TranslateTo(0, 0, 600, Easing.SinIn)
        );
        
        _isLoaded = true;
    }
}