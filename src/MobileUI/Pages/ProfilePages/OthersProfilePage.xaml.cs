using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;
using SSW.Rewards.Shared.DTOs.ActivityFeed;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.Pages;

public partial class OthersProfilePage : ContentPage
{
    private bool _initialised;

    private OthersProfileViewModel viewModel;

    public OthersProfilePage(OthersProfileViewModel vm, LeaderViewModel leader)
    {
        InitializeComponent();
        viewModel = vm;

        viewModel.SetUser(leader);

        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }

    public OthersProfilePage(OthersProfileViewModel vm, NetworkProfileDto networking)
    {
        InitializeComponent();
        viewModel = vm;

        viewModel.ShowBalance = false;

        viewModel.SetUser(networking);

        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }
    
    public OthersProfilePage(OthersProfileViewModel vm, ActivityFeedItemDto activityFeedItem)
    {
        InitializeComponent();
        viewModel = vm;

        viewModel.ShowBalance = false;

        viewModel.SetUser(activityFeedItem);

        viewModel.Navigation = Navigation;
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        if (!_initialised)
            await viewModel.Initialise();

        _initialised = true;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.OnDisappearing();
    }
}