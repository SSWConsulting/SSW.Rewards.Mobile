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
        var dbUser = await _context.Users.FirstAsync(x => x.Id == currentUser.Id, cancellationToken);

        var currentDeviceToken = await _context.DeviceTokens.FirstOrDefaultAsync(x => x.User.Id == currentUser.Id, cancellationToken);
        if (currentDeviceToken == null)
        {
            AddToken(request, dbUser);
        }
        else
        {
            UpdateToken(currentDeviceToken, request);
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }

    private void AddToken(UploadDeviceTokenCommand request, User user)
    {
        var deviceTokenModel =
            new DeviceToken
            {
                Token = request.DeviceToken,
                LastTimeUpdated = request.LastTimeUpdated,
                User = user
            };

        _context.DeviceTokens.Add(deviceTokenModel);
    }

    private void UpdateToken(DeviceToken deviceToken, UploadDeviceTokenCommand request)
    {
        deviceToken.Token = request.DeviceToken;
        deviceToken.LastTimeUpdated = request.LastTimeUpdated;
    }
}