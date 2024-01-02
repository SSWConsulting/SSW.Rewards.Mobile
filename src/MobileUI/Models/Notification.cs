namespace SSW.Rewards.Models
{
    public class Notification
    {
        public string Message { get; set; }
        public NotificationType Type { get; set; }
    }

    public enum NotificationType
    {
        Alert,
        Event
    }
}
