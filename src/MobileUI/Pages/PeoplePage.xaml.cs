namespace SSW.Rewards.Mobile.Pages;

public partial class PeoplePage
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
        await _viewModel.Initialise();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.PageInView = false;
        _viewModel.ScrollToRequested -= ScrollToIndex;
    }

    private void ScrollToIndex(object sender, ScrollToEventArgs e)
    {
        PicCarousel.ScrollTo(e.Index, -1, e.Position, e.Animate);
    }
}