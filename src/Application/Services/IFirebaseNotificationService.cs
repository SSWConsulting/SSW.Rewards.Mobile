namespace SSW.Rewards.Application.Services;

public interface IFirebaseNotificationService
{
    Task SendNotificationAsync<T>(T messagePayload, int userId, string notificationTitle, string notificationMessage, CancellationToken cancellationToken);
}
