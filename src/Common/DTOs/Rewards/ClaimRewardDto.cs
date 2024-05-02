using SSW.Rewards.Shared.DTOs.AddressTypes;

namespace SSW.Rewards.Shared.DTOs.Rewards;

public class ClaimRewardDto
{
    public string Code { get; set; }
    public int Id { get; set; }
    
    public Address Address { get; set; }
    public bool InPerson { get; set; } = true;
    public int UserId { get; set; }
    public bool IsPendingRedemption { get; set; }
}
