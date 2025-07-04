using System.ComponentModel.DataAnnotations;

namespace SSW.Rewards.Domain.Common;

public abstract class BaseAuditableEntity : BaseEntity
{
    public DateTime LastModifiedUtc { get; set; }
    [MaxLength(40)]
    public string? LastModifiedBy { get; set; }

    public DateTime? DeletedUtc { get; set; }
    [MaxLength(40)]
    public string? DeletedBy { get; set; }
}
