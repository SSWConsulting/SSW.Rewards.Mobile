namespace Microsoft.Extensions.DependencyInjection.Notifications.Commands.UploadDeviceToken;

public class UploadDeviceTokenCommand : IRequest<Unit>
{
    public string DeviceToken { get; set; }
    public DateTime LastTimeUpdated { get; set; }
}

public class Handler : IRequestHandler<UploadDeviceTokenCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly IUserService _userService;

    public Handler(IApplicationDbContext context, IUserService userService)
    {
        _context = context;
        _userService = userService;
    }

    public async Task<Unit> Handle(UploadDeviceTokenCommand request, CancellationToken cancellationToken)
    {
        var currentUser = _userService.GetCurrentUser();
        var deviceTokenModel =
            new DeviceToken
            {
                Token = request.DeviceToken,
                LastTimeUpdated = request.LastTimeUpdated,
                UserId = currentUser.Id
            };

        _context.DeviceTokens.Add(deviceTokenModel);
        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}