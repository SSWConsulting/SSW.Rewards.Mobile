namespace SSW.Rewards.Mobile.Services;

public interface IPushNotificationActionService : INotificationActionService
{
    event EventHandler<PushNotificationAction> ActionTriggered;
}