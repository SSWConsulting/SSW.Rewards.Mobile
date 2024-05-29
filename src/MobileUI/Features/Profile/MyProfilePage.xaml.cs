using CommunityToolkit.Mvvm.ComponentModel;
using SSW.Rewards.Mobile.ViewModels.ProfileViewModels;

namespace SSW.Rewards.Mobile.Pages;

public partial class MyProfilePage
{
    private MyProfileViewModel viewModel;

    public MyProfilePage(MyProfileViewModel vm)
    {
        InitializeComponent();
        viewModel = vm;
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