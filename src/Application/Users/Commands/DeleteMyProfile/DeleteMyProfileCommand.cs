using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Users.Commands.Common;

namespace SSW.Rewards.Application.Users.Commands.DeleteMyProfile;

public class DeleteMyProfileCommand : IRequest { }

public class DeleteMyProfileCommandHandler : IRequestHandler<DeleteMyProfileCommand>
{
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly IApplicationDbContext _applicationDbContext;
    private readonly ILogger<DeleteMyProfileCommandHandler> _logger;

    private readonly DeleteProfileOptions _options;

    public DeleteMyProfileCommandHandler(
        IEmailService emailService,
        ICurrentUserService currentUserService,
        IApplicationDbContext applicationDbContext,
        IOptions<DeleteProfileOptions> options,
        ILogger<DeleteMyProfileCommandHandler> logger)
    {
        _emailService = emailService;
        _currentUserService = currentUserService;
        _applicationDbContext = applicationDbContext;
        _logger = logger;
        _options = options.Value;
    }

    public async Task Handle(DeleteMyProfileCommand request, CancellationToken cancellationToken)
    {
        var userName = _currentUserService.GetUserFullName();
        var userEmail = _currentUserService.GetUserEmail();

        var user = await _applicationDbContext.Users.FirstOrDefaultAsync(u => u.Email == userEmail, cancellationToken);

        var deletionRequest = new OpenProfileDeletionRequest { User = user };

        _applicationDbContext.OpenProfileDeletionRequests.Add(deletionRequest);

        await _applicationDbContext.SaveChangesAsync(cancellationToken);

        var model = new DeleteProfileEmail
        {
            UserName = userName,
            UserEmail = userEmail,
            RewardsTeamEmail = _options.Recipient
        };

        var sent = await _emailService.SendProfileDeletionRequest(model, cancellationToken);

        if (!sent)
        {
            _logger.LogError("Could not send profile delete request message for {userName}, {email}", userName, userEmail);
            throw new Exception($"Failed to send profile delete request message for {userName}, {userEmail}");
        }
    }
}
