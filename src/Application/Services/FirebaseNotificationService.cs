using System.Text.Json;
using FirebaseAdmin;
using FirebaseAdmin.Messaging;
using Google.Apis.Auth.OAuth2;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Services;

public class FirebaseNotificationServiceOptions
{
    public string? FirebaseCredentials { get; set; }
}

public interface IFirebaseNotificationService
{
    Task SendNotificationAsync<T>(T messagePayload, int userId, CancellationToken cancellationToken);
}

public class FirebaseNotificationService : IFirebaseNotificationService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly Lazy<FirebaseApp> _lazyFirebaseApp;

    public FirebaseNotificationService(IOptions<FirebaseNotificationServiceOptions> options, IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;

        // Initialize Lazy<FirebaseApp> with a factory method that handles the creation.
        // This factory will only be executed once, the first time _lazyFirebaseApp.Value is accessed.
        _lazyFirebaseApp = new Lazy<FirebaseApp>(() =>
        {
            // Check if an app is already initialized (e.g., by another instance or part of the app)
            // FirebaseApp.DefaultInstance is static and shared.
            if (FirebaseApp.DefaultInstance == null)
            {
                return FirebaseApp.Create(new AppOptions()
                {
                    Credential = GoogleCredential.FromJson(options.Value.FirebaseCredentials)
                });
            }
            return FirebaseApp.DefaultInstance; // Return the existing instance
        });
    }

    private async Task SendNotificationToDevice(Message message, CancellationToken cancellationToken)
    {
        // Accessing .Value will trigger the initialization factory if it hasn't run yet.
        _ = _lazyFirebaseApp.Value; 
        await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);
    }

    public async Task SendNotificationAsync<T>(T messagePayload, int userId, CancellationToken cancellationToken)
    {
        // Ensure Firebase is initialized by accessing the Value property of Lazy<FirebaseApp>
        // This needs to be done before any Firebase SDK calls, SendNotificationToDevice handles this.

        List<string> deviceTokens = await _dbContext.DeviceTokens
            .Where(dt => dt.User.Id == userId && !string.IsNullOrEmpty(dt.Token))
            .OrderByDescending(dt => dt.LastTimeUpdated) // Sort by LastTimeUpdated descending
            .Select(dt => dt.Token)
            .Distinct()
            .ToListAsync(cancellationToken);

        if (!deviceTokens.Any())
        {
            // Log or handle the case where no device tokens are found for the user
            // Consider logging this event for monitoring.
            return;
        }

        string payloadJson = JsonSerializer.Serialize(messagePayload);
        foreach (string token in deviceTokens)
        {
            var message = new Message()
            {
                Token = token,
                Data = new Dictionary<string, string>()
                {
                    { "payload", payloadJson }
                },
                // You can also set Notification property for standard display notifications
                // Notification = new Notification
                // {
                // Title = "Your Notification Title",
                // Body = "Your Notification Body"
                // }
            };

            try
            {
                await SendNotificationToDevice(message, cancellationToken);
            }
            catch (FirebaseMessagingException ex)
            {
                // Log the exception, and potentially handle specific error codes
                // e.g., 'unregistering' tokens that are no longer valid.
                // For example, if ex.MessagingErrorCode == MessagingErrorCode.Unregistered
                // you might want to remove the token from the database.
                Console.WriteLine($"Error sending notification to token {token}: {ex.Message}"); // Replace with proper logging
            }
        }
    }
}
