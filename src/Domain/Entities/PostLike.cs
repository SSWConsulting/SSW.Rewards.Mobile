namespace SSW.Rewards.Domain.Entities;

public class PostLike : BaseEntity
{
    public int PostId { get; set; }

    public Post Post { get; set; } = null!;

    public int UserId { get; set; }

    public User User { get; set; } = null!;
}
