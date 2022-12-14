using SSW.Rewards.Controls;

namespace SSW.Rewards.Pages;

[XamlCompilation(XamlCompilationOptions.Compile)]
public partial class PeoplePage : ContentPage
{
    private readonly DevProfilesViewModel _viewModel;

    public PeoplePage(DevProfilesViewModel viewModel)
    {
        _viewModel = viewModel;
        _viewModel.Navigation = Navigation;
        BindingContext = _viewModel;
        InitializeComponent();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        _viewModel.PageInView = true;
        _viewModel.ScrollToRequested += ScrollToIndex;
        _viewModel.ShowSnackbar += ShowSnackbar;
        await _viewModel.Initialise();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PageInView = false;
        _viewModel.ScrollToRequested -= ScrollToIndex;
        _viewModel.ShowSnackbar -= ShowSnackbar;
    }

    private void ScrollToIndex(object sender, int index)
    {
        PicCarousel.ScrollTo(index);
    }

    private async void ShowSnackbar(object sender, ShowSnackbarEventArgs e)
    {
        PeoplePageSnackbar.Options = e.Options;
        await PeoplePageSnackbar.ShowSnackbar();
    }
}