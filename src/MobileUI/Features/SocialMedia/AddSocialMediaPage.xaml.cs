namespace SSW.Rewards.Mobile.PopupPages;

public partial class AddSocialMediaPage
{
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public AddSocialMediaPage(IFirebaseAnalyticsService firebaseAnalyticsService, IServiceProvider provider,
        int socialMediaPlatformId, string currentUrl)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
        BindingContext = ActivatorUtilities.CreateInstance<AddSocialMediaViewModel>(provider, socialMediaPlatformId, currentUrl);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("AddSocialMediaPage");
    }
}