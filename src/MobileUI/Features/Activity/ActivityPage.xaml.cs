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
        await _viewModel.Initialise();
    }
}