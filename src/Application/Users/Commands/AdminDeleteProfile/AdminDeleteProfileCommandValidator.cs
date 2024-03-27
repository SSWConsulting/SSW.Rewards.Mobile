namespace SSW.Rewards.Application.Users.Commands.AdminDeleteProfile;

public class AdminDeleteProfileCommandValidator : AbstractValidator<AdminDeleteProfileCommand>
{
    private readonly IApplicationDbContext _applicationDbContext;

    public AdminDeleteProfileCommandValidator(IApplicationDbContext applicationDbContext)
    {
        _applicationDbContext = applicationDbContext;

        RuleFor(c => c.Profile.UserId).MustAsync(HaveCorrespondingDeletionRequest);
    }

    private async Task<bool> HaveCorrespondingDeletionRequest(int userId, CancellationToken cancellationToken)
    {
        return await _applicationDbContext.OpenProfileDeletionRequests.AnyAsync(r => r.UserId == userId, cancellationToken);
    }
}
