using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Notifications.Commands.RequestNotification
{
    //TODO: Pending V2 admin portal
    public class RequestNotificationCommand : IRequest<Unit>
    {
        public string Text { get; set; }
        public string Action { get; set; }
        public string[] Tags { get; set; } = Array.Empty<string>();
        public bool Silent { get; set; }
        
        public class RequestNotificationCommandHandler : IRequestHandler<RequestNotificationCommand, Unit>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;
            private readonly ICurrentUserService _currentUserService;
            private readonly INotificationService _notificationService;
            
            public RequestNotificationCommandHandler(IMapper mapper, 
                ISSWRewardsDbContext context, 
                ICurrentUserService currentUserService, 
                INotificationService notificationService)
            {
                _mapper = mapper;
                _context = context;
                _currentUserService = currentUserService;
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
              
                //var notificationEntry = new Domain.Entities.Notifications
                //{
                //    Message = notification.Text,
                //    NotificationTag = string.Join(",", notification.Tags),
                //    NotificationAction = notification.Action
                //    SentByStaffMember = _currentUserService.GetUserEmail()
                //};                
                //await _context.Notifications.AddAsync(notificationEntry, cancellationToken);
                //await _context.SaveChangesAsync(cancellationToken);
                
                return Unit.Value;
            }
        }
    }
}