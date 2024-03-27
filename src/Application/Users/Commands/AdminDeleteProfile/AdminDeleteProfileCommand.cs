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

    public AdminDeleteProfileHandler(IApplicationDbContext dbContext, IEmailService emailService)
    {
        _dbContext = dbContext;
        _emailService = emailService;
    }

    public async Task Handle(AdminDeleteProfileCommand request, CancellationToken cancellationToken)
    {
        var deletionRequest = await _dbContext.OpenProfileDeletionRequests
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.UserId == request.Profile.UserId, cancellationToken);


    }
}
