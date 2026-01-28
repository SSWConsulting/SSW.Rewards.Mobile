namespace SSW.Rewards.Mobile.Pages;

public partial class PostListPage
{
    private readonly PostListViewModel _viewModel;

    public PostListPage(PostListViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.InitialiseAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _viewModel.OnDisappearing();
    }
}
