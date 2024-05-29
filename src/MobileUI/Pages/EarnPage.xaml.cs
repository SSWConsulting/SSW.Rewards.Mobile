namespace SSW.Rewards.Mobile.Pages;

public partial class EarnPage
{
    private readonly EarnViewModel _viewModel;
    
    private bool _isLoaded;

    public EarnPage(EarnViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Initialise();
        await Animate();
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