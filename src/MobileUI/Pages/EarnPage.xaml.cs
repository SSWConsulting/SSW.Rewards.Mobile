namespace SSW.Rewards.Mobile.Pages;

public partial class EarnPage : ContentPage
{
    private readonly EarnViewModel _viewModel;

    private IDispatcherTimer _timer;
    
    private bool _isLoaded;

    public EarnPage(EarnViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
        _timer = Application.Current.Dispatcher.CreateTimer();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Initialise();
        await Animate();
        BeginAutoScroll();
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer.Stop();
        _timer.Tick -= OnScrollTick;
    }

    private void BeginAutoScroll()
    {
        _timer.Interval = TimeSpan.FromSeconds(3);
        _timer.Tick += OnScrollTick;
        _timer.Start();
    }
    
    private void OnScrollTick(object sender, object args)
    {
        MainThread.BeginInvokeOnMainThread(Scroll);
    }
    
    private void Scroll()
    {
        var count = _viewModel.CarouselQuizzes.Count;
        
        if (count > 0)
            Carousel.Position = (Carousel.Position + 1) % count;
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