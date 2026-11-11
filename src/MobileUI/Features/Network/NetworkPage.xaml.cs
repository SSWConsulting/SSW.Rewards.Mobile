namespace SSW.Rewards.Mobile.Pages;

public partial class NetworkPage
{
    private readonly NetworkPageViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public NetworkPage(NetworkPageViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        _viewModel = viewModel;
        _viewModel.Navigation = Navigation;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("NetworkPage");
        await _viewModel.Initialise();
    }
}