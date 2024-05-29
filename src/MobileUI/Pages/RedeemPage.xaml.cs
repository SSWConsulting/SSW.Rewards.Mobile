namespace SSW.Rewards.Mobile.Pages;

public partial class RedeemPage : ContentPage
{
    private readonly RewardsViewModel _viewModel;
    
    private bool _isLoaded;

    public RedeemPage(RewardsViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Initialise();
    }
    
    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}