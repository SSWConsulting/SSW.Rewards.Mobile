namespace SSW.Rewards.Application.Services;

public interface IFirebaseNotificationService
{
    Task<FirebaseNotificationResult> SendNotificationAsync<T>(int userId, string notificationTitle, string notificationMessage, string? imageUrl, T messagePayload, CancellationToken cancellationToken);
    Task<FirebaseNotificationResult> SendNotificationAsync(int userId, string notificationTitle, string notificationMessage, string? imageUrl, string payloadJson, CancellationToken cancellationToken);
}
