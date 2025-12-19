namespace SSW.Rewards.Shared.DTOs.Posts;

public class CreatePostDto
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public bool SendNotification { get; set; }
    public bool IsPublished { get; set; }
}
