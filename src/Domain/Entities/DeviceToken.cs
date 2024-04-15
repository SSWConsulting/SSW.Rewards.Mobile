namespace SSW.Rewards.Domain.Entities;

public class DeviceToken : BaseEntity
{
    public string Token { get; set; }
    public DateTime LastTimeUpdated { get; set; }
    public User User { get; set; }
}