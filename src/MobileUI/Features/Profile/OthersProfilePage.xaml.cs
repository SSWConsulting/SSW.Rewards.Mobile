using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

namespace SSW.Rewards.Mobile.Pages;

public partial class OthersProfilePage
{
    private readonly OthersProfileViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    
    public OthersProfilePage(OthersProfileViewModel vm, IFirebaseAnalyticsService firebaseAnalyticsService, int userId)
    {
        InitializeComponent();
        _viewModel = vm;

        _viewModel.ShowBalance = false;
        
        _viewModel.SetUser(userId);

        _viewModel.Navigation = Navigation;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("OthersProfilePage");
        await _viewModel.Initialise();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}