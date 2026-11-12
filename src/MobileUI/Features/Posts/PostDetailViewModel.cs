#nullable enable

using AsyncAwaitBestPractices;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Mobile.Common;
using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class PostDetailViewModel : BaseViewModel, IQueryAttributable
{
    private const string PostTitle = "Post";
    private const string OfflineMessage = "You're offline. This post will load once you're back online.";
    private const string GenericMessage = "There seems to be a problem loading this post. Please try again soon.";
    private const string DeletedMessage = "This post is no longer available.";

    private readonly IPostsService _postsService;
    private readonly IFileCacheService _fileCacheService;
    private readonly IConnectivityService _connectivity;
    private readonly IAlertService _alertService;
    private readonly IApiExceptionHandler _apiExceptionHandler;
    private readonly IAppNavigator _navigator;
    private readonly OfflineAwareListErrorHandler _errorHandler;
    private readonly ILogger<PostDetailViewModel> _logger;

    // One cache entry per post opened; expire them so they don't accumulate until logout.
    private static readonly TimeSpan CacheExpiry = TimeSpan.FromDays(30);

    private int _postId;
    private CancellationTokenSource? _loadCts;

    public PostDetailViewModel(
        IPostsService postsService,
        IFileCacheService fileCacheService,
        IConnectivityService connectivity,
        IAlertService alertService,
        IApiExceptionHandler apiExceptionHandler,
        IAppNavigator navigator,
        OfflineAwareListErrorHandler errorHandler,
        ILogger<PostDetailViewModel> logger)
    {
        _postsService = postsService;
        _fileCacheService = fileCacheService;
        _connectivity = connectivity;
        _alertService = alertService;
        _apiExceptionHandler = apiExceptionHandler;
        _navigator = navigator;
        _errorHandler = errorHandler;
        _logger = logger;
    }

    [ObservableProperty]
    public partial PostDetailDto? Post { get; set; }

    [ObservableProperty]
    public partial string NewComment { get; set; } = string.Empty;

    [ObservableProperty]
    public partial bool IsLiked { get; set; }

    [ObservableProperty]
    public partial int LikesCount { get; set; }

    [ObservableProperty]
    public partial bool IsLoadingAction { get; set; }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("PostId", out var postId))
        {
            _postId = Convert.ToInt32(postId);
            LoadPostAsync().SafeFireAndForget(ex => _logger.LogError(ex, "Error loading post details"));
        }
    }

    private async Task LoadPostAsync()
    {
        _loadCts?.Cancel();
        _loadCts?.Dispose();
        _loadCts = new CancellationTokenSource();
        var ct = _loadCts.Token;

        // The spinner covers an empty page only; a cached post renders straight away.
        IsBusy = Post is null;
        try
        {
            var result = await CachedItemLoader.LoadAsync<PostDetailDto>(
                _fileCacheService,
                $"PostDetail_{_postId}",
                token => _postsService.GetPostById(_postId, token),
                (post, fromCache) =>
                {
                    // Reloads after a like or comment skip the cached copy: it predates the change.
                    if (!fromCache || Post is null)
                    {
                        Show(post);
                    }

                    return Task.CompletedTask;
                },
                ct,
                CacheExpiry);

            if (result.Error.IsNotFound())
            {
                // Deleted on the server (the loader has dropped the cached copy): stop showing it, so
                // the user can't like or comment on a post that no longer exists.
                Post = null;
                await _alertService.DisplayAlertAsync(PostTitle, DeletedMessage, "OK");
                await _navigator.GoBackAsync();
                return;
            }

            // A failure behind the cached post stays quiet.
            await _errorHandler.HandleAsync(result, userRequestedNewData: false, PostTitle, OfflineMessage, GenericMessage);
        }
        finally
        {
            // A superseding load owns the spinner now.
            if (!ct.IsCancellationRequested)
            {
                IsBusy = false;
            }
        }
    }

    private void Show(PostDetailDto post)
    {
        Post = post;
        IsLiked = post.CurrentUserLiked;
        LikesCount = post.LikesCount;
        Title = post.Title;
    }

    [RelayCommand]
    private async Task ToggleLikeAsync()
    {
        if (IsLoadingAction || !await EnsureOnlineAsync("like this post"))
            return;

        IsLoadingAction = true;
        try
        {
            var result = await _postsService.ToggleLike(_postId, CancellationToken.None);

            IsLiked = result;
            LikesCount += result ? 1 : -1;

            if (Post != null)
            {
                await LoadPostAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like");
            await ShowWriteErrorAsync(ex, "Failed to update like");
        }
        finally
        {
            IsLoadingAction = false;
        }
    }

    [RelayCommand]
    private async Task AddCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewComment) || IsLoadingAction || !await EnsureOnlineAsync("comment"))
            return;

        IsLoadingAction = true;
        try
        {
            var commentId = await _postsService.AddComment(_postId, NewComment.Trim(), CancellationToken.None);

            if (commentId > 0 && Post != null)
            {
                await LoadPostAsync();
                NewComment = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            await ShowWriteErrorAsync(ex, "Failed to add comment");
        }
        finally
        {
            IsLoadingAction = false;
        }
    }

    [RelayCommand]
    private async Task DeleteCommentAsync(PostCommentDto comment)
    {
        if (IsLoadingAction || !await EnsureOnlineAsync("delete a comment"))
            return;

        bool confirm = await _alertService.DisplayConfirmationAsync(
            "Delete Comment",
            "Are you sure you want to delete this comment?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        IsLoadingAction = true;
        try
        {
            await _postsService.DeleteComment(comment.Id, CancellationToken.None);
            await LoadPostAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment");
            await ShowWriteErrorAsync(ex, "Failed to delete comment");
        }
        finally
        {
            IsLoadingAction = false;
        }
    }

    // A 401 goes to re-login (as loads do) instead of failing every later tap the same way.
    private async Task ShowWriteErrorAsync(Exception ex, string message)
    {
        if (!await _apiExceptionHandler.TryHandleAsync(ex))
        {
            await _alertService.DisplayAlertAsync("Error", message, "OK");
        }
    }

    // Writes stay online-only (no outbox), so say why up front instead of failing the request.
    private async Task<bool> EnsureOnlineAsync(string action)
    {
        if (_connectivity.IsOnline)
        {
            return true;
        }

        await _alertService.DisplayAlertAsync("You're offline", $"Connect to the internet to {action}.", "OK");
        return false;
    }
}
