namespace SSW.Rewards.Mobile.PopupPages;

public partial class AddSocialMediaPage
{
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly AddSocialMediaViewModel _vm;
    private readonly string _currentUrl;
    private readonly int _socialMediaPlatformId;

    public AddSocialMediaPage(IFirebaseAnalyticsService firebaseAnalyticsService, IServiceProvider provider,
        int socialMediaPlatformId, string currentUrl)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _socialMediaPlatformId = socialMediaPlatformId;
        _currentUrl = currentUrl;
        InitializeComponent();
        _vm = ActivatorUtilities.CreateInstance<AddSocialMediaViewModel>(provider);
        BindingContext = _vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("AddSocialMediaPage");
        await _vm.InitialiseAsync(_currentUrl, _socialMediaPlatformId);
    }
}