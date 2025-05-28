using SSW.Rewards.Application.Services;

namespace SSW.Rewards.Application.Notifications.Commands;

public class SendNotificationToUserCommand : IRequest
{
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? DataPayload { get; set; }
}

public class SendNotificationToUserCommandHandler : IRequestHandler<SendNotificationToUserCommand>
{
    private readonly IFirebaseNotificationService _firebaseNotificationService;

    public SendNotificationToUserCommandHandler(IFirebaseNotificationService firebaseNotificationService)
    {
        _firebaseNotificationService = firebaseNotificationService;
    }

    public async Task Handle(SendNotificationToUserCommand request, CancellationToken cancellationToken)
    {
        request.DataPayload ??= string.Empty;
        await _firebaseNotificationService.SendNotificationAsync(
            request.UserId,
            request.Title,
            request.Message,
            request.DataPayload,
            cancellationToken);
    }
}
