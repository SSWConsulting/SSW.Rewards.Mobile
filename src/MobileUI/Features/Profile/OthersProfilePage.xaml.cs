using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

namespace SSW.Rewards.Mobile.Pages;

[QueryProperty(nameof(UserId), nameof(UserId))]
public partial class OthersProfilePage
{
    private readonly OthersProfileViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    
    public string UserId { get; set; }
    
    public OthersProfilePage(OthersProfileViewModel vm, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        Title = "Profile";
        InitializeComponent();
        _viewModel = vm;
        _viewModel.ShowBalance = false;
        _viewModel.Navigation = Navigation;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("OthersProfilePage");
        var userId = int.Parse(UserId);
        _viewModel.SetUser(userId);
        await _viewModel.Initialise();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}