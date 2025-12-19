namespace SSW.Rewards.Mobile.Pages;

public partial class ActivityPage
{
    private readonly ActivityPageViewModel _viewModel;
    private readonly IFirebaseAnalyticsService _firebaseAnalyticsService;
    private readonly IServiceProvider _serviceProvider;
    private PostListPage _postListPage;

    public ActivityPage(ActivityPageViewModel viewModel, IFirebaseAnalyticsService firebaseAnalyticsService, IServiceProvider serviceProvider)
    {
        _viewModel = viewModel;
        _viewModel.Navigation = Navigation;
        _firebaseAnalyticsService = firebaseAnalyticsService;
        _serviceProvider = serviceProvider;

        BindingContext = _viewModel;
        Title = "Activity";
        InitializeComponent();

        // Subscribe to property changed to handle segment switching
        _viewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(_viewModel.ShowPosts))
        {
            if (_viewModel.ShowPosts)
            {
                LoadPostsPage();
            }
        }
    }

    private void LoadPostsPage()
    {
        if (_postListPage == null)
        {
            _postListPage = ActivatorUtilities.CreateInstance<PostListPage>(_serviceProvider);
        }

        PostsContentView.Content = _postListPage.Content;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        _firebaseAnalyticsService.Log("ActivityPage");
        await _viewModel.LoadFeed();
    }
}