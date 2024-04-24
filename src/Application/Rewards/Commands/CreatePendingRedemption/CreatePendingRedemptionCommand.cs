using System.Text;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Commands;

public class CreatePendingRedemptionCommand : IRequest<CreatePendingRedemptionResult>
{
    public int UserId { get; set; }
    public int Id { get; set; }
}

public class CreatePendingRedemptionCommandHandler : IRequestHandler<CreatePendingRedemptionCommand, CreatePendingRedemptionResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<CreatePendingRedemptionCommandHandler> _logger;
    private readonly IDateTime _dateTime;
    private readonly ICurrentUserService _currentUserService;

    public CreatePendingRedemptionCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<CreatePendingRedemptionCommandHandler> logger,
            IDateTime dateTime,
            ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _currentUserService = currentUserService;
    }

    public async Task<CreatePendingRedemptionResult> Handle(CreatePendingRedemptionCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context
            .Rewards
            .Where(r => r.Id == request.Id)
            .FirstOrDefaultAsync(cancellationToken);

        if (reward == null)
        {
            _logger.LogError("Reward not found with id: {Id}", request.Id);
            return new CreatePendingRedemptionResult
            {
                status = RewardStatus.NotFound
            };
        }
        
        User? user = await _context.Users
            .Where(u => u.Email == _currentUserService.GetUserEmail())
            .Include(u => u.UserAchievements)
            .ThenInclude(ua => ua.Achievement)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogError("User with ID {UserId} claiming the reward {RewardId}({RewardName}) does not exist!", request.UserId, reward.Id, reward.Name);
            return new CreatePendingRedemptionResult
            {
                status = RewardStatus.Error
            };
        }

        var userRewards = await _context
                .UserRewards
                .Include(ur => ur.Reward)
                .Where(ur => ur.UserId == user.Id)
                .ToListAsync(cancellationToken);

        int pointsUsed = userRewards.Sum(ur => ur.Reward.Cost);
        int totalPoints = user.UserAchievements.Sum(ua => ua.Achievement.Value);
        int balance = totalPoints - pointsUsed;

        if (balance < reward.Cost)
        {
            _logger.LogInformation("User does not have enough points to claim reward");
            return new CreatePendingRedemptionResult
            {
                status = RewardStatus.NotEnoughPoints
            };
        }

        var codeData = Encoding.ASCII.GetBytes($"pnd:{Guid.NewGuid().ToString()}");
        var code = Convert.ToBase64String(codeData);
        
        try
        {
            user.PendingRedemptions.Add(new PendingRedemption
            {
                Code = code,
                ClaimedAt = _dateTime.Now,
                RewardId = reward.Id,
                UserId = user.Id
            });

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return new CreatePendingRedemptionResult
            {
                status = RewardStatus.Error
            };
        }

        var rewardModel = _mapper.Map<RewardDto>(reward);

        return new CreatePendingRedemptionResult
        {
            status = RewardStatus.Pending,
            Reward = rewardModel,
            Code = code
        };
    }
}