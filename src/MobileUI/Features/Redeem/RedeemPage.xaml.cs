namespace SSW.Rewards.Mobile.Pages;

public partial class RedeemPage : ContentPage
{
    private readonly RedeemViewModel _viewModel;

    private IDispatcherTimer _timer;
    
    private bool _isLoaded;

    public RedeemPage(RedeemViewModel viewModel)
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
        var count = _viewModel.CarouselRewards.Count;
        
        if (count > 0)
            Carousel.Position = (Carousel.Position + 1) % count;
    }
}