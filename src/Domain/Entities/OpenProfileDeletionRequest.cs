namespace SSW.Rewards.Domain.Entities;

public class OpenProfileDeletionRequest : BaseAuditableEntity
{
    public int UserId { get; set; }
    public required User User { get; set; }
}
