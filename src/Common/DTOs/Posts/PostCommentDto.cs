namespace SSW.Rewards.Shared.DTOs.Posts;

public class PostCommentDto
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string UserAvatar { get; set; } = string.Empty;
    public string Comment { get; set; } = string.Empty;
    public DateTime CreatedUtc { get; set; }
}
