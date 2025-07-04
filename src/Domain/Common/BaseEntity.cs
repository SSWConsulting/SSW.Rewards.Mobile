using System.ComponentModel.DataAnnotations;

namespace SSW.Rewards.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }

    public DateTime CreatedUtc { get; set; }
    [MaxLength(40)]
    public string? CreatedBy { get; set; }
}
