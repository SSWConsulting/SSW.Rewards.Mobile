using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SSW.Rewards.Application.Users.Commands.DeleteMyProfile;

public class DeleteMyProfileCommand : IRequest { }

public class DeleteMyProfileCommandHandler : IRequestHandler<DeleteMyProfileCommand, Unit>
{
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<DeleteMyProfileCommandHandler> _logger;

    private readonly DeleteProfileOptions _options;

    public DeleteMyProfileCommandHandler(
        IEmailService emailService,
        ICurrentUserService currentUserService,
        IOptions<DeleteProfileOptions> options,
        ILogger<DeleteMyProfileCommandHandler> logger)
    {
        _emailService = emailService;
        _currentUserService = currentUserService;
        _logger = logger;
        _options = options.Value;
    }

    public async Task<Unit> Handle(DeleteMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userName = _currentUserService.GetUserFullName();
        var userEmail = _currentUserService.GetUserEmail();

        var model = new DeleteProfileEmail
        {
            UserName = userName,
            UserEmail = userEmail,
            RewardsTeamEmail = _options.Recipient
        };

        var sent = await _emailService.SendProfileDeletionRequest(model, cancellationToken);

        if (sent)
        {
            return Unit.Value;
        }
        else
        {
            _logger.LogError("Could not send profile delete request message for {userName}, {email}", userName, userEmail);
            throw new Exception($"Failed to send profile delete request message for {userName}, {userEmail}");
        }
    }
}
