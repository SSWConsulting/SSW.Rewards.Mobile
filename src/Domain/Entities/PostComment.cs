namespace SSW.Rewards.Domain.Entities;

public class PostComment : BaseAuditableEntity
{
    public int PostId { get; set; }

    public Post Post { get; set; } = null!;

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public string Comment { get; set; } = string.Empty;
}
