using SSW.Rewards.Application.Common.Models;

namespace SSW.Rewards.Application.Notifications.Commands.UpdateInstallation;

public class UpdateInstallationCommand : IRequest<Unit>
{
    public string InstallationId { get; set; }
    public string Platform { get; set; }
    public string PushChannel { get; set; }
    public List<string> Tags { get; set; } 
    
    public class UpdateInstallationCommandHandler : IRequestHandler<UpdateInstallationCommand,Unit>
    {
        private readonly INotificationService _notificationService;
        
        public UpdateInstallationCommandHandler(INotificationService notificationService)
        {
            _notificationService = notificationService;
        }
        public async Task<Unit> Handle(UpdateInstallationCommand request, CancellationToken cancellationToken)
        {
            var deviceInstallation = new DeviceInstall
            {
                InstallationId = request.InstallationId,
                Platform = request.Platform,
                PushChannel = request.PushChannel,
                Tags = request.Tags
            };
            
            await _notificationService
                .CreateOrUpdateInstallationAsync(deviceInstallation, cancellationToken);
            
            return Unit.Value;
        }
    }
}