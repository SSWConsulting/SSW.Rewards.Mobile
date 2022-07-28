namespace SSW.Rewards.Application.Common.Models;
public class DigitalRewardEmail
{
    public string RecipientName { get; set; } = string.Empty;
    public string RecipientEmail { get; set; } = string.Empty;
    public string RewardName { get; set; } = string.Empty;
    public string VoucherCode { get; set; } = string.Empty;
}
