namespace SSW.Rewards.Application.Services;

public interface IFirebaseNotificationService
{
    Task<bool> SendNotificationAsync<T>(int userId, string notificationTitle, string notificationMessage, T messagePayload, CancellationToken cancellationToken);
    Task<bool> SendNotificationAsync(int userId, string notificationTitle, string notificationMessage, string payloadJson, CancellationToken cancellationToken);
}
