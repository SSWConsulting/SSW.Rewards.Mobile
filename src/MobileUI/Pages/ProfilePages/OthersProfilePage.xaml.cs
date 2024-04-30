using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Mobile.Pages;

public partial class OthersProfilePage
{
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

    protected override async void OnAppearing()
    {
        await viewModel.Initialise();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        viewModel.OnDisappearing();
    }
}