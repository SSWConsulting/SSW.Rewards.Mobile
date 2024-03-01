namespace SSW.Rewards.Shared.DTOs.Rewards;

public class ClaimRewardDto
{
    public string Code { get; set; }
    public int Id { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string AddressSuburb { get; set; }
    public string AddressState { get; set; }
    public string AddressPostcode { get; set; }
    public bool InPerson { get; set; }
}
