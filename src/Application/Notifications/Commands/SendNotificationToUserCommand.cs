using SSW.Rewards.Application.Services;

namespace SSW.Rewards.Application.Notifications.Commands;

public class SendNotificationToUserCommand : IRequest<NotificationSentResponse>
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? DataPayload { get; set; }
}

public class SendNotificationToUserCommandHandler : IRequestHandler<SendNotificationToUserCommand, NotificationSentResponse>
{
    private readonly IFirebaseNotificationService _firebaseNotificationService;

    public SendNotificationToUserCommandHandler(IFirebaseNotificationService firebaseNotificationService)
    {
        _firebaseNotificationService = firebaseNotificationService;
    }

    public async Task<NotificationSentResponse> Handle(SendNotificationToUserCommand request, CancellationToken cancellationToken)
    {
        request.DataPayload ??= string.Empty;
        var stats = await _firebaseNotificationService.SendNotificationAsync(
            request.UserId,
            request.Title,
            request.Message,
            request.ImageUrl,
            request.DataPayload,
            cancellationToken);

        return new NotificationSentResponse
        {
            UsersToNotify = 1,
            NotificationsSent = stats.Sent
        };
    }
}
