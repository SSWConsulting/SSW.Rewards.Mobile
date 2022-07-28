using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SSW.Rewards.Domain.Entities;
public class Notification : BaseEntity
{
    public int SentByStaffMemberId { get; set; }
    public User SentByStaffMember { get; set; }
    public string Message { get; set; }
    public string NotificationTag { get; set; }
    public string NotificationAction { get; set; } = string.Empty;
}
