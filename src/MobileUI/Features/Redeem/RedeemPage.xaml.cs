namespace SSW.Rewards.Mobile.Pages;

public partial class RedeemPage
{
    private readonly RedeemViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public RedeemPage(RedeemViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("RedeemPage");
        await _viewModel.Initialise();
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}