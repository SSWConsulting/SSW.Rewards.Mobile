namespace SSW.Rewards.Mobile.Pages;

public partial class ActivityPage
{
    private readonly ActivityPageViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public ActivityPage(ActivityPageViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        _viewModel = viewModel;
        _viewModel.Navigation = Navigation;
        _firebaseAnalyticsService = firebaseAnalyticsService;

        BindingContext = _viewModel;
        Title = "Activity";
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("ActivityPage");
        await _viewModel.LoadFeed();
    }

    protected override void OnNavigatedFrom(NavigatedFromEventArgs args)
    {
        base.OnNavigatedFrom(args);

        // Still on a stack means a page was pushed on top; it's only gone for good once popped.
        if (!Navigation.NavigationStack.Contains(this) && !Navigation.ModalStack.Contains(this))
        {
            _viewModel.Dispose();
        }
    }
}