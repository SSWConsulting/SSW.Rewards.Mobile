namespace SSW.Rewards.Application.Rewards.Common;
public class RewardViewModel
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Cost { get; set; }
    public string ImageUri { get; set; }
    public RewardType RewardType { get; set; }
}
