using System.Text;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Commands;

public class CancelPendingRedemptionCommand : IRequest<CancelPendingRedemptionResult>
{
    public int Id { get; set; }
}

public class CancelPendingRedemptionCommandHandler : IRequestHandler<CancelPendingRedemptionCommand, CancelPendingRedemptionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<CreatePendingRedemptionCommandHandler> _logger;
    private readonly ICurrentUserService _currentUserService;

    public CancelPendingRedemptionCommandHandler(
            IApplicationDbContext context,
            ILogger<CreatePendingRedemptionCommandHandler> logger,
            ICurrentUserService currentUserService)
    {
        _context = context;
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<CancelPendingRedemptionResult> Handle(CancelPendingRedemptionCommand request, CancellationToken cancellationToken)
    {
        User? user = await _context.Users
            .Where(u => u.Email == _currentUserService.GetUserEmail())
            .Include(pr => pr.PendingRedemptions)
            .Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogError("User with email {Email} does not exist!", _currentUserService.GetUserEmail());
            return new CancelPendingRedemptionResult
            {
                Status = RewardStatus.Error
            };
        }

        var pendingRedemption = user.PendingRedemptions.FirstOrDefault(pr => pr.RewardId == request.Id && !pr.CancelledByUser && !pr.CancelledByAdmin && !pr.Completed);

        if (pendingRedemption == null)
        {
            _logger.LogError("Pending redemption for reward id {Id} does not exist!", request.Id);
            return new CancelPendingRedemptionResult
            {
                Status = RewardStatus.Error
            };
        }

        pendingRedemption.CancelledByUser = true;
        await _context.SaveChangesAsync(cancellationToken);
        
        return new CancelPendingRedemptionResult
        {
            Status = RewardStatus.Cancelled
        };
    }
}