namespace SSW.Rewards.Shared.DTOs.Posts;

public class PostDetailDto : PostDto
{
    public List<PostCommentDto> Comments { get; set; } = [];
}
