﻿namespace SSW.Rewards.Application.Notifications.Commands;

public class NotificationSentResponse
{
    public int UsersToNotify { get; set; }
    public int NotificationsSent { get; set; }
    
    public bool WasArchived { get; set; }

    public static NotificationSentResponse Empty { get; } = new();

    public static NotificationSentResponse SendingNotificationTo(int numberOfUsers)
        => new() { UsersToNotify = numberOfUsers };

    public static NotificationSentResponse Archived()
        => new() { WasArchived = true };
}
