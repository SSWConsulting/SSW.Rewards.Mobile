namespace SSW.Rewards.Mobile.Pages;

public partial class RedeemPage
{
    private readonly RedeemViewModel _viewModel;
    
    private bool _isLoaded;

    public RedeemPage(RedeemViewModel viewModel)
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