using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Notifications.Commands.RequestNotification;

//TODO: Pending V2 admin portal
public class RequestNotificationCommand : IRequest<Unit>
{
    public string Text { get; set; }
    public string Action { get; set; }
    public string[] Tags { get; set; } = Array.Empty<string>();
    public bool Silent { get; set; }
    
    public class RequestNotificationCommandHandler : IRequestHandler<RequestNotificationCommand, Unit>
    {
        private readonly IApplicationDbContext _context;
        private readonly IUserService _userService;
        private readonly INotificationService _notificationService;
        
        public RequestNotificationCommandHandler(
            IApplicationDbContext context, 
            IUserService userService,
            INotificationService notificationService)
        {
            _context = context;
            _userService = userService;
            _notificationService = notificationService;
        }
        
        public async Task<Unit> Handle(RequestNotificationCommand request, CancellationToken cancellationToken)
        {
            var notification = new NotificationRequest
            {
                Text = request.Text,
                Action = request.Action,
                Silent = request.Silent,
                Tags = request.Tags
            };
            
            if ((notification.Silent && string.IsNullOrWhiteSpace(notification?.Action)) ||
                (!notification.Silent && string.IsNullOrWhiteSpace(notification?.Text)))
                throw new Exception("Bad Request");

            var success = await _notificationService
                .RequestNotificationAsync(notification, cancellationToken);
        
            if (!success)
                throw new Exception("Request Failed");

            var user = await this._userService.GetCurrentUser(cancellationToken);

            var notificationEntry = new Domain.Entities.Notification
            {
                Message             = notification.Text,
                NotificationTag     = string.Join(",", notification.Tags),
                NotificationAction  = notification.Action,
                SentByStaffMemberId = user.Id
            };
            await _context.Notifications.AddAsync(notificationEntry, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}