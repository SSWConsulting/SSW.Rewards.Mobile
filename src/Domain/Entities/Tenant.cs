namespace SSW.Rewards.Domain.Entities;

public class Tenant : BaseAuditableEntity
{
    public string Name { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
    public string? Description { get; set; }

    public TenantSettings? Settings { get; set; }
}