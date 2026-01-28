namespace SSW.Rewards.Shared.DTOs.Posts;

public class PostDto
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool SendNotification { get; set; }
    public bool IsPublished { get; set; }
    public DateTime? PublishedDateUtc { get; set; }
    public DateTime CreatedUtc { get; set; }
    public DateTime LastModifiedUtc { get; set; }
    public int LikesCount { get; set; }
    public int CommentsCount { get; set; }
    public bool CurrentUserLiked { get; set; }

    public static bool IsEqual(PostDto? a, PostDto? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;

        return a.Id == b.Id &&
               a.Title == b.Title &&
               a.Content == b.Content &&
               a.ImageUrl == b.ImageUrl &&
               a.SendNotification == b.SendNotification &&
               a.IsPublished == b.IsPublished &&
               a.PublishedDateUtc == b.PublishedDateUtc &&
               a.CreatedUtc == b.CreatedUtc &&
               a.LastModifiedUtc == b.LastModifiedUtc &&
               a.LikesCount == b.LikesCount &&
               a.CommentsCount == b.CommentsCount &&
               a.CurrentUserLiked == b.CurrentUserLiked;
    }
}
