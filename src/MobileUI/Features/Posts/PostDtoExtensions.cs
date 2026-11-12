using SSW.Rewards.Shared.DTOs.Posts;

namespace SSW.Rewards.Mobile.ViewModels;

public static class PostDtoExtensions
{
    /// <summary>Anti-flicker check for the posts list: compares every field the list card shows.</summary>
    public static bool HasSameDisplayAs(this PostDto post, PostDto other) =>
        post.Id == other.Id
        && post.Title == other.Title
        && post.Content == other.Content
        && post.ImageUrl == other.ImageUrl
        && post.PublishedDateUtc == other.PublishedDateUtc
        && post.LikesCount == other.LikesCount
        && post.CommentsCount == other.CommentsCount
        && post.CommentsDisabled == other.CommentsDisabled;
}
