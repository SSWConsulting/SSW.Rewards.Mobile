using CommunityToolkit.Mvvm.Messaging;
using SSW.Rewards.Mobile.Messages;

namespace SSW.Rewards.Mobile.Pages;

public partial class SettingsPage : ContentPage
{
    public SettingsPage(SettingsViewModel viewModel)
    {
        BindingContext = viewModel;
        InitializeComponent();
    }
        
    protected override void OnAppearing()
    {
        base.OnAppearing();
        SettingsViewModel.Initialise();
    }
}