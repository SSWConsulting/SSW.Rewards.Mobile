namespace SSW.Rewards.Mobile.Pages;

public partial class ActivityPage : ContentPage
{
    private readonly ActivityPageViewModel _viewModel;

    public ActivityPage(ActivityPageViewModel viewModel)
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