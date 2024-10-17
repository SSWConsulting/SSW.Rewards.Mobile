using CommunityToolkit.Mvvm.ComponentModel;
using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

namespace SSW.Rewards.Mobile.Pages;

public partial class MyProfilePage
{
    private readonly MyProfileViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;

    public MyProfilePage(MyProfileViewModel vm, IFirebaseAnalyticsService firebaseAnalyticsService)
    {
        InitializeComponent();
        _viewModel = vm;
        _viewModel.Navigation = Navigation;
        BindingContext = _viewModel;
        _firebaseAnalyticsService = firebaseAnalyticsService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("MyProfilePage");
        await _viewModel.Initialise();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}