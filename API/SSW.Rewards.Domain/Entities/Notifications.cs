namespace SSW.Rewards.Domain.Entities
{
    public class Notifications : Entity
    {
        public string SentByStaffMember { get; set; }
        public string Message { get; set; }
        public string NotificationTag { get; set; }
        public string NotificationAction { get; set; } = string.Empty;
    }
}