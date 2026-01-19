using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.Logging;
using SSW.Rewards.ApiClient.Services;
using SSW.Rewards.Shared.DTOs.Posts;
using IUserService = SSW.Rewards.Mobile.Services.IUserService;

namespace SSW.Rewards.Mobile.ViewModels;

public partial class PostDetailViewModel : BaseViewModel, IQueryAttributable
{
    private readonly IPostsService _postsService;
    private readonly IUserService _userService;
    private readonly ILogger<PostDetailViewModel> _logger;

    private int _postId;
    private int _currentUserId;
    private bool _isStaff;

    public PostDetailViewModel(IPostsService postsService, IUserService userService, ILogger<PostDetailViewModel> logger)
    {
        _postsService = postsService;
        _userService = userService;
        _logger = logger;

        _userService.MyUserIdObservable().Subscribe(userId => _currentUserId = userId);
        _userService.IsStaffObservable().Subscribe(isStaff => _isStaff = isStaff);
    }

    [ObservableProperty]
    private PostDetailDto _post;

    [ObservableProperty]
    private string _newComment = string.Empty;

    [ObservableProperty]
    private bool _isLiked;

    [ObservableProperty]
    private int _likesCount;

    [ObservableProperty]
    private bool _isLoadingAction;

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("PostId", out var postId))
        {
            _postId = Convert.ToInt32(postId);
            Task.Run(async () => await LoadPostAsync());
        }
    }

    private async Task LoadPostAsync()
    {
        if (IsBusy)
            return;

        IsBusy = true;
        try
        {
            var result = await _postsService.GetPostById(_postId, CancellationToken.None);

            if (result != null)
            {
                Post = result;
                IsLiked = result.CurrentUserLiked;
                LikesCount = result.LikesCount;
                Title = result.Title;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading post details");
            await Shell.Current.DisplayAlertAsync("Error", "Failed to load post details", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ToggleLikeAsync()
    {
        if (IsLoadingAction)
            return;

        IsLoadingAction = true;
        try
        {
            var result = await _postsService.ToggleLike(_postId, CancellationToken.None);

            IsLiked = result;
            LikesCount += result ? 1 : -1;

            // Update the main Post object as well
            if (Post != null)
            {
                // Reload to get updated data
                await LoadPostAsync();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling like");
            await Shell.Current.DisplayAlertAsync("Error", "Failed to update like", "OK");
        }
        finally
        {
            IsLoadingAction = false;
        }
    }

    [RelayCommand]
    private async Task AddCommentAsync()
    {
        if (string.IsNullOrWhiteSpace(NewComment) || IsLoadingAction)
            return;

        IsLoadingAction = true;
        try
        {
            var commentId = await _postsService.AddComment(_postId, NewComment.Trim(), CancellationToken.None);

            if (commentId > 0 && Post != null)
            {
                // Reload the post to get updated comments
                await LoadPostAsync();
                NewComment = string.Empty;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding comment");
            await Shell.Current.DisplayAlertAsync("Error", "Failed to add comment", "OK");
        }
        finally
        {
            IsLoadingAction = false;
        }
    }

    [RelayCommand]
    private async Task DeleteCommentAsync(PostCommentDto comment)
    {
        if (IsLoadingAction)
            return;

        // Check if user can delete (own comment or staff)
        if (!CanDeleteComment(comment))
        {
            await Shell.Current.DisplayAlertAsync("Error", "You cannot delete this comment", "OK");
            return;
        }

        // Confirm deletion
        bool confirm = await Shell.Current.DisplayAlertAsync(
            "Delete Comment",
            "Are you sure you want to delete this comment?",
            "Delete",
            "Cancel");

        if (!confirm)
            return;

        IsLoadingAction = true;
        try
        {
            // Use admin endpoint if staff is deleting another user's comment
            if (_isStaff && comment.UserId != _currentUserId)
            {
                await _postsService.AdminDeleteComment(comment.Id, CancellationToken.None);
            }
            else
            {
                await _postsService.DeleteComment(comment.Id, CancellationToken.None);
            }

            // Reload the post to get updated comments
            await LoadPostAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting comment");
            await Shell.Current.DisplayAlertAsync("Error", "Failed to delete comment", "OK");
        }
        finally
        {
            IsLoadingAction = false;
        }
    }

    public bool CanDeleteComment(PostCommentDto comment)
    {
        // Staff can delete any comment, users can only delete their own
        return _isStaff || comment.UserId == _currentUserId;
    }
}
