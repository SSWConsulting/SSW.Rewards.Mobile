using System;

namespace SSW.Rewards.Application.Notifications.Queries.GetNotificationHistoryList
{
    public class NotificationHistoryDto
    {
        public string Message { get; set; }
        public DateTime CreatedDate { get; set; }
        public string EmailAddress { get; set; }
    }
}