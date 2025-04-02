namespace SSW.Rewards.PopupPages;

public partial class ProfilePicturePage
{
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public ProfilePicturePage(IFirebaseAnalyticsService firebaseAnalyticsService, ProfilePictureViewModel profilePictureViewModel)
    {
        _firebaseAnalyticsService = firebaseAnalyticsService;
        InitializeComponent();
        profilePictureViewModel.Navigation = Navigation;
        BindingContext = profilePictureViewModel;
    }
    
    protected override void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("ProfilePicturePage");
    }
}
