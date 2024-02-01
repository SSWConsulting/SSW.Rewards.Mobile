namespace SSW.Rewards.Mobile.Pages;

public partial class EarnPage : ContentPage
{
    private readonly EarnViewModel _viewModel;

    private IDispatcherTimer _timer;

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
        BeginAutoScroll();
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _timer.Stop();
    }

    private void BeginAutoScroll()
    {
        _timer = Application.Current.Dispatcher.CreateTimer();
        _timer.Interval = TimeSpan.FromSeconds(3);
        _timer.Tick += (s,e) => Scroll();
        _timer.Start();
    }
    
    private void Scroll()
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            var count = _viewModel.CarouselQuizzes.Count;
            Carousel.Position = (Carousel.Position + 1) % count;
        });
    }
}