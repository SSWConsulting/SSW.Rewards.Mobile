namespace SSW.Rewards.Application.Common.Models;

public class DigitalRewardEmail
{
    public int ClaimId { get; set; }
    public string RecipientName { get; set; }
    public string RecipientEmail { get; set; }
    public string RewardName { get; set; }
    public string VoucherCode { get; set; }
}
