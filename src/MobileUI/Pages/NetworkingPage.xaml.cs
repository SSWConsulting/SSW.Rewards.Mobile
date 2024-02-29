namespace SSW.Rewards.Mobile.Pages;

public partial class NetworkingPage : ContentPage
{
    private readonly NetworkingPageViewModel _viewModel;

    public NetworkingPage(NetworkingPageViewModel viewModel)
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