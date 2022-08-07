namespace SSW.Rewards.Domain.Entities;
public class UserRole : BaseEntity
{
    public int UserId { get; set; }
    public User User { get; set; } = new();
    public int RoleId { get; set; }
    public Role Role { get; set; } = new();
}
