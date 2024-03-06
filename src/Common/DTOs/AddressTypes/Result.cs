namespace SSW.Rewards.Shared.DTOs.AddressTypes;

public class Result
{
    public string type { get; set; }
    public string id { get; set; }
    public double score { get; set; }
    public Address address { get; set; }
}