using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Notifications.Commands.RequestNotification;
public class RequestNotification : IRequest<Unit>
{
    public string Text { get; set; }
    public string Action { get; set; }
    public IList<string> Tags { get; set; }
    public bool Silent { get; set; }

    public RequestNotification(string text, string action, IList<string> tags, bool silent)
    {
        Text    = text;
        Action  = action;
        Tags    = tags;
        Silent  = silent;
    }
}

public class RequestNotificationHandler : IRequestHandler<RequestNotification, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserService _userService;
    private readonly INotificationService _notificationService;

    public RequestNotificationHandler(
        IApplicationDbContext context,
        IUserService userService,
        INotificationService notificationService)
    {
        _context = context;
        _userService = userService;
        _notificationService = notificationService;
    }

    public async Task<Unit> Handle(RequestNotification request, CancellationToken cancellationToken)
    {
        var notification = new NotificationRequest
        {
            Text    = request.Text,
            Action  = request.Action,
            Silent  = request.Silent,
            Tags    = request.Tags.ToArray()
        };

        if ((notification.Silent && string.IsNullOrWhiteSpace(notification?.Action)) ||
            (!notification.Silent && string.IsNullOrWhiteSpace(notification?.Text)))
            throw new Exception("Bad Request");

        await _notificationService
            .RequestNotificationAsync(notification, cancellationToken);

        var userId = await _userService.GetCurrentUserId(cancellationToken);

        var notificationEntry = new Notification
        {
            Message             = notification.Text,
            NotificationTag     = string.Join(",", notification.Tags),
            NotificationAction  = notification.Action,
            SentByStaffMemberId = userId
        };
        await _context.Notifications.AddAsync(notificationEntry, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}