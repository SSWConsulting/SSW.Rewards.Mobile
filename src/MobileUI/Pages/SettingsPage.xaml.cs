namespace SSW.Rewards.Mobile.Pages;

public partial class SettingsPage
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