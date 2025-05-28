namespace SSW.Rewards.Application.Services;

public interface IFirebaseNotificationService
{
    Task SendNotificationAsync<T>(int userId, string notificationTitle, string notificationMessage, T messagePayload, CancellationToken cancellationToken);
    Task SendNotificationAsync(int userId, string notificationTitle, string notificationMessage, string payloadJson, CancellationToken cancellationToken);
}
