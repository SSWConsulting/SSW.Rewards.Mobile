namespace SSW.Rewards.Mobile.Pages;

public partial class SettingsPage
{
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public SettingsPage(SettingsViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        BindingContext = viewModel;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
    }
        
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("SettingsPage");
        SettingsViewModel.Initialise();
    }
}