using Microsoft.Extensions.Options;
using SSW.Rewards.Application.Users.Commands.Common;
using SSW.Rewards.Application.Users.Commands.DeleteMyProfile;
using SSW.Rewards.Shared.DTOs.Users;

namespace SSW.Rewards.Application.Users.Commands.AdminDeleteProfile;

public class AdminDeleteProfileCommand : IRequest
{
    public required AdminDeleteProfileDto Profile { get; set; }

}

public class AdminDeleteProfileHandler : IRequestHandler<AdminDeleteProfileCommand>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEmailService _emailService;
    private readonly string _emailSender;

    public AdminDeleteProfileHandler(
        IApplicationDbContext dbContext,
        IEmailService emailService,
        IOptions<DeleteProfileOptions> options)
    {
        _dbContext = dbContext;
        _emailService = emailService;
        _emailSender = options.Value.Recipient;
    }

    public async Task Handle(AdminDeleteProfileCommand request, CancellationToken cancellationToken)
    {
        var deletionRequest = await _dbContext.OpenProfileDeletionRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == request.Profile.UserId, cancellationToken);

        if (deletionRequest?.User is null)
        {
            throw new DirectoryNotFoundException($"User with id {request.Profile.UserId} either not found or not marked for deletion.");
        }

        var emailModel = new DeleteProfileEmail
        {
            RewardsTeamEmail = _emailSender,
            UserEmail = deletionRequest!.User!.Email!,
            UserName = deletionRequest!.User!.FullName!,
            RequestDate = deletionRequest!.CreatedUtc.ToLocalTime()
        };

        AnonymiseUserPII(deletionRequest.User);

        deletionRequest.User.Activated = false;

        _dbContext.OpenProfileDeletionRequests.Remove(deletionRequest);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _emailService.SendProfileDeletionConfirmation(emailModel, cancellationToken);

        // TODO: update SSW.Identity. See: https://github.com/SSWConsulting/SSW.IdentityServer/issues/224
    }

    public void AnonymiseUserPII(User user)
    {
        if (user != null)
        {
            var guid = Guid.NewGuid().ToString("N"); // N format removes hyphens
                        
            var part1 = guid.Substring(0, 8);
            var part2 = guid.Substring(8, 8);
            var part3 = guid.Substring(16, 8);
            var part4 = guid.Substring(24, 8);

            user.FullName = $"Anon-{part1} Anon-{part2}";
            user.Email = $"{part3}@{part4}.com";
        }
    }

}
