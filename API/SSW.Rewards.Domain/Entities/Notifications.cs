using System.ComponentModel.DataAnnotations.Schema;

namespace SSW.Rewards.Domain.Entities
{
    public class Notifications : Entity
    {
        public int SentByStaffMemberId { get; set; }
        [ForeignKey("SentByStaffMemberId")]
        public User SentByStaffMember { get; set; }
        public string Message { get; set; }
        public string NotificationTag { get; set; }
        public string NotificationAction { get; set; } = string.Empty;

        
    }
}