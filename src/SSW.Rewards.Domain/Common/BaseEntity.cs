using System.ComponentModel.DataAnnotations.Schema;

namespace SSW.Rewards.Domain.Common;

public abstract class BaseEntity
{
    public int Id { get; set; }
    public System.DateTime CreatedUtc { get; set; }
}
