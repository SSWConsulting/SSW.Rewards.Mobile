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
    private int _page = 1;
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
            return;

        await LoadPostsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        IsRefreshing = true;
        try
        {
            _page = 1;
            _limitReached = false;
            await LoadPostsAsync(replace: true);
        }
        finally
        {
            IsRefreshing = false;
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
            var result = await _postsService.GetPosts(_page, PageSize, publishedOnly: true, CancellationToken.None);

            if (result == null || result.Items == null || !result.Items.Any())
            {
                _limitReached = true;
                return;
            }

            if (replace)
            {
                Posts.ReplaceRange(result.Items);
            }
            else
            {
                Posts.AddRange(result.Items);
            }

            // Check if we've reached the end using IsLastPage
            if (result.IsLastPage)
            {
                _limitReached = true;
            }
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

    [RelayCommand]
    private async Task PostTappedAsync(PostDto post)
    {
        if (post == null)
            return;

        var parameters = new Dictionary<string, object>
        {
            { "PostId", post.Id }
        };

        await Shell.Current.GoToAsync($"postdetail", parameters);
    }
}
