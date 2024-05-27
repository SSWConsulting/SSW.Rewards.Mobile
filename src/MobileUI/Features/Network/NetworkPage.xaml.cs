namespace SSW.Rewards.Mobile.Pages;

public partial class NetworkPage : ContentPage
{
    private readonly NetworkPageViewModel _viewModel;

    public NetworkPage(NetworkPageViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.Navigation = Navigation;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.Initialise();
    }
}