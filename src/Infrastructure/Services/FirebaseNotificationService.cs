using System.Text.Json;
using FirebaseAdmin.Messaging;
using Hangfire;
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
    private readonly IBackgroundJobClient _backgroundJobClient;

    public FirebaseNotificationService(
        IApplicationDbContext dbContext,
        IFirebaseInitializerService firebaseInitializerService,
        ILogger<FirebaseNotificationService> logger,
        IBackgroundJobClient backgroundJobClient)
    {
        _dbContext = dbContext;
        _firebaseInitializerService = firebaseInitializerService;
        _logger = logger;
        _backgroundJobClient = backgroundJobClient;
    }

    public async Task<bool> SendNotificationAsync<T>(int userId, string notificationTitle, string notificationMessage, T messagePayload, CancellationToken cancellationToken)
    {
        string payloadJson = JsonSerializer.Serialize(messagePayload);
        return await SendNotificationAsync(userId, notificationTitle, notificationMessage, payloadJson, cancellationToken);
    }

    public async Task<bool> SendNotificationAsync(int userId, string notificationTitle, string notificationMessage, string payloadJson, CancellationToken cancellationToken)
    {
        var deviceTokens = await _dbContext.DeviceTokens
            .Where(dt => dt.User.Id == userId && !string.IsNullOrEmpty(dt.Token))
            .OrderByDescending(dt => dt.LastTimeUpdated)
            .Select(dt => new { dt.Id, dt.Token })
            .GroupBy(dt => dt.Token)
            .Select(g => g.First())
            .ToListAsync(cancellationToken);

        if (!deviceTokens.Any())
        {
            _logger.LogWarning("No device tokens found for User ID {UserId}. Notification not sent.", userId);
            return false;
        }

        _logger.LogInformation("Preparing to send notification titled '{NotificationTitle}' to User ID {UserId} for {DeviceCount} device(s).", notificationTitle, userId, deviceTokens.Count);

        bool atLeastOneSent = false;
        foreach (var deviceToken in deviceTokens)
        {
            var message = new Message()
            {
                Token = deviceToken.Token,
                Data = new Dictionary<string, string> { ["payload"] = payloadJson },
                Notification = new FirebaseNotification
                {
                    Title = notificationTitle,
                    Body = notificationMessage
                }
            };

            try
            {
                 _logger.LogDebug("Sending notification to token {DeviceToken} for User ID {UserId}", deviceToken.Token, userId);

                await SendNotificationToDevice(message, cancellationToken);

                atLeastOneSent = true;
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
                    }
                    catch (Exception dbEx)
                    {
                        _logger.LogError(dbEx, "Error removing invalid device token {DeviceToken} for User ID {UserId} from database.", deviceToken, userId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error sending notification to token {DeviceToken} for User ID {UserId}", deviceToken, userId);
            }
        }

        return atLeastOneSent;
    }

    private async Task SendNotificationToDevice(Message message, CancellationToken cancellationToken)
    {
        // Ensure Firebase is initialized
        _firebaseInitializerService.Initialize();
        await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }
}
