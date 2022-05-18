namespace SSW.Rewards.Domain.Entities
{
    public class UserRole : Entity
    {
        public int UserId { get; set; }

        public User User { get; set; }

        public int RoleId { get; set; }

        public Role Role { get; set; }
    }
}
