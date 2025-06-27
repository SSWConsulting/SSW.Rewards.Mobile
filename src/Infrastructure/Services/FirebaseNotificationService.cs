using System.Text.Json;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Common.Extensions;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Services;
using FirebaseNotification = FirebaseAdmin.Messaging.Notification;

namespace SSW.Rewards.Infrastructure.Services;

public class FirebaseNotificationService : IFirebaseNotificationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFirebaseInitializerService _firebaseInitializerService;
    private readonly ILogger<FirebaseNotificationService> _logger;

    public FirebaseNotificationService(
        IApplicationDbContext dbContext,
        IFirebaseInitializerService firebaseInitializerService,
        ILogger<FirebaseNotificationService> logger)
    {
        _dbContext = dbContext;
        _firebaseInitializerService = firebaseInitializerService;
        _logger = logger;
    }

    public async Task<FirebaseNotificationResult> SendNotificationAsync<T>(int userId, string notificationTitle, string notificationMessage, string? imageUrl, T messagePayload, CancellationToken cancellationToken)
    {
        string payloadJson = JsonSerializer.Serialize(messagePayload);
        return await SendNotificationAsync(userId, notificationTitle, notificationMessage, imageUrl, payloadJson, cancellationToken);
    }

    public async Task<FirebaseNotificationResult> SendNotificationAsync(int userId, string notificationTitle, string notificationMessage, string? imageUrl, string payloadJson, CancellationToken cancellationToken)
    {
        FirebaseNotificationResult result = new();
        var deviceTokens = await _dbContext.DeviceTokens
            .Where(dt => dt.User.Id == userId && !string.IsNullOrEmpty(dt.Token))
            .OrderByDescending(dt => dt.LastTimeUpdated)
            .Select(dt => new { dt.Id, dt.Token })
            .GroupBy(dt => dt.Token)
            .Select(g => g.First())
            .ToListAsync(cancellationToken);

        result.Devices = deviceTokens.Count;
        if (!deviceTokens.Any())
        {
            _logger.LogWarning("No device tokens found for User ID {UserId}. Notification not sent.", userId);
            return result;
        }

        _logger.LogInformation("Preparing to send notification titled '{NotificationTitle}' to User ID {UserId} for {DeviceCount} device(s).", notificationTitle, userId, deviceTokens.Count);

        foreach (var deviceToken in deviceTokens)
        {
            var message = new Message()
            {
                Token = deviceToken.Token,
                Data = new Dictionary<string, string> { ["payload"] = payloadJson },
                Notification = new FirebaseNotification
                {
                    Title = notificationTitle,
                    Body = notificationMessage,
                    ImageUrl = imageUrl
                }
            };

            try
            {
                 _logger.LogDebug("Sending notification to token {DeviceToken} for User ID {UserId}", deviceToken.Token, userId);

                await SendNotificationToDevice(message, cancellationToken);

                ++result.Sent;
            }
            catch (FirebaseMessagingException ex)
            {
                _logger.LogError(ex, "Error sending Firebase notification to token {DeviceToken} for User ID {UserId}. MessagingErrorCode: {MessagingErrorCode}", deviceToken, userId, ex.MessagingErrorCode);

                if (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
                {
                    _logger.LogInformation("Removing invalid device token {DeviceToken} for User ID {UserId} due to MessagingErrorCode {MessagingErrorCode}.", deviceToken, userId, ex.MessagingErrorCode);

                    try
                    {
                        await _dbContext.DeviceTokens
                            .TagWithContext("DeletingInvalidDeviceToken")
                            .Where(x => x.Id == deviceToken.Id)
                            .ExecuteDeleteAsync(cancellationToken);

                        ++result.Removed;
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Error removing invalid device token {DeviceToken} for User ID {UserId} from database.", deviceToken, userId);

                        ++result.Failed;
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending notification to token {DeviceToken} for User ID {UserId}", deviceToken, userId);

                ++result.Failed;
            }
        }

        return result;
    }

    private async Task SendNotificationToDevice(Message message, CancellationToken cancellationToken)
    {
        // Ensure Firebase is initialized
        _firebaseInitializerService.Initialize();
        await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }
}
