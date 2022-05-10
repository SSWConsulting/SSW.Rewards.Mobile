using System;
using SSW.Rewards.Models;

namespace SSW.Rewards.Services
{
    public interface IPushNotificationActionService : INotificationActionService
    {
        event EventHandler<PushNotificationAction> ActionTriggered;
    }
}
