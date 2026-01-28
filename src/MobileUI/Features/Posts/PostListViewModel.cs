using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Mobile.Services;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class PostListViewModel : BaseViewModel, IDisposable
{
    private readonly IPostsService _postsService;
    private readonly ILogger<PostListViewModel> _logger;
    private readonly IFileCacheService _fileCacheService;

    private const int PageSize = 20;
    private const string CacheKey = "PostsList";
    private int _page = 0;
    private bool _limitReached;
    private bool _disposed;

    public PostListViewModel(IPostsService postsService, ILogger<PostListViewModel> logger, IFileCacheService fileCacheService)
    {
        Title = "Posts";
        _postsService = postsService;
        _logger = logger;
        _fileCacheService = fileCacheService;
    }

    public AdvancedObservableCollection<PostDto> Posts { get; } = new();

    [ObservableProperty]
    private bool _isRefreshing;

    public async Task InitialiseAsync()
    {
        Posts.InitializeInitialCaching(_fileCacheService, CacheKey, () => true);
        Posts.CompareItems = PostDto.IsEqual;
        Posts.OnCollectionUpdated += OnPostsUpdated;
        Posts.OnError += OnPostsError;

        if (!Posts.IsLoaded)
        {
            await LoadPostsAsync();
        }
    }

    public void OnDisappearing()
    {
        Posts.OnCollectionUpdated -= OnPostsUpdated;
        Posts.OnError -= OnPostsError;
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
            await Posts.LoadAsync(async ct => await FetchPostsData(ct), reload: replace);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading posts");
            _limitReached = true;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<List<PostDto>> FetchPostsData(CancellationToken ct)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        cts.CancelAfter(TimeSpan.FromSeconds(30));

        var result = await _postsService.GetPosts(_page, PageSize, publishedOnly: true, searchTerm: null, sortBy: null, sortDirection: null, cts.Token);

        if (result == null || result.Items == null || !result.Items.Any())
        {
            _limitReached = true;
            return [];
        }

        // Check if we've reached the end using IsLastPage
        if (result.IsLastPage)
        {
            _limitReached = true;
        }

        return result.Items.ToList();
    }

    private void OnPostsUpdated(List<PostDto> posts, bool isFromCache)
    {
        MainThread.BeginInvokeOnMainThread(() =>
        {
            IsBusy = false;
            IsRefreshing = false;
        });
    }

    private bool OnPostsError(Exception ex)
    {
        _limitReached = true;
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            if (!Posts.IsLoaded)
            {
                string userMessage;

                if (ex is HttpRequestException)
                {
                    userMessage = "Unable to load posts due to a network issue. Please check your internet connection and try again.";
                }
                else if (ex is OperationCanceledException)
                {
                    userMessage = "Loading posts timed out. Please check your connection.";
                }
                else
                {
                    userMessage = "An unexpected error occurred while loading posts. Please try again later.";
                }

                await Shell.Current.DisplayAlert("Oops...", userMessage, "OK");
            }

            IsBusy = false;
            IsRefreshing = false;
        });
        return true;
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

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                OnDisappearing();
                Posts.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
