namespace SSW.Rewards.Domain.Entities;
public class PostalAddress : BaseEntity
{
    public string BuildingNameOrNumber { get; set; }
    public string StreetName { get; set; }
    public string Municipality { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}
