namespace SSW.Rewards.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public DateTime CreatedUtc { get; set; }
}
