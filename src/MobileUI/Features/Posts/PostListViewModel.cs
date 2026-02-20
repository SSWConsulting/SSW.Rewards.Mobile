using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class PostListViewModel : BaseViewModel
{
    private readonly IPostsService _postsService;
    private readonly ILogger<PostListViewModel> _logger;

    private const int PageSize = 20;
    private int _page = 0;
    private bool _limitReached;

    public PostListViewModel(IPostsService postsService, ILogger<PostListViewModel> logger)
    {
        Title = "Posts";
        _postsService = postsService;
        _logger = logger;
    }

    public ObservableRangeCollection<PostDto> Posts { get; } = [];

    [ObservableProperty]
    private bool _isRefreshing;

    public async Task InitialiseAsync()
    {
        if (Posts.Count > 0)
        {
            return;
        }

        await LoadPostsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            _page = 0;
            _limitReached = false;
            await LoadPostsAsync(replace: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error refreshing posts");
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync("Error", "Failed to refresh posts. Please try again.", "OK"));
        }
        finally
        {
            await MainThread.InvokeOnMainThreadAsync(() => IsRefreshing = false);
        }
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        if (IsBusy || _limitReached)
            return;

        _page++;
        await LoadPostsAsync();
    }

    private async Task LoadPostsAsync(bool replace = false)
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
            var result = await _postsService.GetPosts(_page, PageSize, publishedOnly: true, searchTerm: null, sortBy: null, sortDirection: null, cts.Token);

            if (result == null || result.Items == null || !result.Items.Any())
            {
                _limitReached = true;
                return;
            }

            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                if (replace)
                {
                    Posts.ReplaceRange(result.Items);
                }
                else
                {
                    Posts.AddRange(result.Items);
                }
            });

            // Check if we've reached the end using IsLastPage
            if (result.IsLastPage)
            {
                _limitReached = true;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Loading posts timed out");
            _limitReached = true;
            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.DisplayAlertAsync("Timeout", "The request took too long. Please check your connection.", "OK"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading posts");
            _limitReached = true;
            MainThread.BeginInvokeOnMainThread(async () =>
                await Shell.Current.DisplayAlertAsync("Error", $"Failed to load posts: {ex.Message}", "OK"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task PostTappedAsync(PostDto post)
    {
        if (post == null)
            return;

        try
        {
            var parameters = new Dictionary<string, object>
            {
                { "PostId", post.Id }
            };

            await Shell.Current.GoToAsync($"postdetail", parameters);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error navigating to post detail");
            await MainThread.InvokeOnMainThreadAsync(async () =>
                await Shell.Current.DisplayAlertAsync("Error", "Failed to open post", "OK"));
        }
    }
}
