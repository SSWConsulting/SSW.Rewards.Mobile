namespace SSW.Rewards.Domain.Entities;

public class Post : BaseAuditableEntity
{
    public string Title { get; set; } = string.Empty;

    public string Content { get; set; } = string.Empty;

    public string? ImageUrl { get; set; }

    public bool SendNotification { get; set; }

    public bool IsPublished { get; set; }

    public DateTime? PublishedDateUtc { get; set; }

    public ICollection<PostLike> PostLikes { get; set; } = new HashSet<PostLike>();

    public ICollection<PostComment> PostComments { get; set; } = new HashSet<PostComment>();
}
