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
}
