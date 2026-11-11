#nullable enable

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class PostListViewModel : BaseViewModel
{
    private const string PostsTitle = "Posts";
    private const string OfflineMessage = "You're offline. Posts will load once you're back online.";
    private const string GenericMessage = "There seems to be a problem loading posts. Please try again soon.";
    private const int PageSize = 20;

    private readonly IAppNavigator _navigator;
    private readonly OfflineAwareListErrorHandler _errorHandler;

    public PagedListSource<PostDto> Posts { get; }

    [ObservableProperty]
    public partial bool IsRefreshing { get; set; }

    public PostListViewModel(
        IPostsService postsService,
        IAppNavigator navigator,
        OfflineAwareListErrorHandler errorHandler,
        IFileCacheService fileCacheService)
    {
        Title = PostsTitle;
        _navigator = navigator;
        _errorHandler = errorHandler;

        Posts = new PagedListSource<PostDto>(new()
        {
            PageSize = PageSize,
            FetchPage = async (skip, take, ct) =>
            {
                var result = await postsService.GetPosts(
                    PageNumberPaging.ToPageIndex(skip, take), take, publishedOnly: true,
                    searchTerm: null, sortBy: null, sortDirection: null, ct);
                return result.Items?.ToList() ?? [];
            },
            AreSame = (a, b) => a.HasSameDisplayAs(b),
            Cache = fileCacheService,
            CacheKey = "PostsCache",
        });
    }

    /// <summary>Loads on first show; retries while only cached posts are on screen (e.g. after being offline).</summary>
    public async Task InitialiseAsync()
    {
        if (Posts.Items.Count > 0 && !Posts.IsShowingCachedData)
        {
            return;
        }

        await LoadPostsAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPostsAsync();
        IsRefreshing = false;
    }

    [RelayCommand]
    private async Task LoadMoreAsync()
    {
        var result = await Posts.LoadMoreAsync();

        // Loaded posts stay on screen, so only a session expiry interrupts scrolling.
        await _errorHandler.HandleAsync(result, userRequestedNewData: false, PostsTitle, OfflineMessage, GenericMessage);
    }

    [RelayCommand]
    private Task PostTappedAsync(PostDto? post)
        => post is null ? Task.CompletedTask : _navigator.GoToPostDetailAsync(post.Id);

    private async Task LoadPostsAsync()
    {
        // The spinner covers an empty list only; cached posts render straight away.
        IsBusy = Posts.Items.Count == 0;
        try
        {
            var result = await Posts.RefreshAsync();

            // A failure behind cached posts stays quiet.
            await _errorHandler.HandleAsync(result, userRequestedNewData: false, PostsTitle, OfflineMessage, GenericMessage);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
