namespace SSW.Rewards.Mobile.Pages;

public partial class EarnPage : ContentPage
{
    private readonly EarnViewModel _viewModel;

    public EarnPage(EarnViewModel viewModel)
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
}