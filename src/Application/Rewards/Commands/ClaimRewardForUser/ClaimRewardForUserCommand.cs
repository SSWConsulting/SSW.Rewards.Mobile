using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Commands;

public class ClaimRewardForUserCommand : IRequest<ClaimRewardResult>
{
    public int UserId { get; set; }
    public string Code { get; set; }
    public bool IsPendingRedemption { get; set; }
}

public class ClaimRewardForUserCommandHandler : IRequestHandler<ClaimRewardForUserCommand, ClaimRewardResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ClaimRewardForUserCommandHandler> _logger;
    private readonly IDateTime _dateTime;
    private readonly ICacheService _cacheService;

    public ClaimRewardForUserCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ClaimRewardForUserCommandHandler> logger,
            IDateTime dateTime,
            ICacheService cacheService)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
        _cacheService = cacheService;
    }

    public async Task<ClaimRewardResult> Handle(ClaimRewardForUserCommand request, CancellationToken cancellationToken)
    {
        Reward? reward = null;
        PendingRedemption? pendingRedemption = null;
        var userId = request.UserId;

        if (request.IsPendingRedemption)
        {
            pendingRedemption = await _context.PendingRedemptions
                .TagWithContext("GetPendingRedemption")
                .Include(pr => pr.Reward)
                .Where(pr => pr.Code == request.Code)
                .FirstOrDefaultAsync(cancellationToken);

            if (pendingRedemption != null)
            {
                if (pendingRedemption.Completed)
                {
                    _logger.LogError("Pending reward was already scanned: {0}", request.Code);
                    return new ClaimRewardResult
                    {
                        status = RewardStatus.Duplicate
                    };
                }

                reward = pendingRedemption.Reward;
                userId = pendingRedemption.UserId;
            }
        }
        else
        {
            reward = await _context.Rewards
                .TagWithContext("GetReward")
                .Where(r => r.Code == request.Code)
                .FirstOrDefaultAsync(cancellationToken);
        }

        if (reward == null)
        {
            _logger.LogError("Reward not found with code: {RewardCode}", request.Code);
            return new ClaimRewardResult
            {
                status = RewardStatus.NotFound
            };
        }

        User? user = await _context.Users
            .TagWithContext("GetUser")
            .Where(u => u.Id == userId)
            .Include(u => u.UserAchievements)
                .ThenInclude(ua => ua.Achievement)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogError("User with ID {UserId} claiming the reward {RewardId}({RewardName}) does not exist!", userId, reward.Id, reward.Name);
            return new ClaimRewardResult
            {
                status = RewardStatus.Error
            };
        }

        var userRewards = await _context.UserRewards
                .TagWithContext("GetUserRewards")
                .Include(ur => ur.Reward)
                .Where(ur => ur.UserId == user.Id)
                .ToListAsync(cancellationToken);

        int pointsUsed = userRewards.Sum(ur => ur.Reward.Cost);
        int totalPoints = user.UserAchievements.Sum(ua => ua.Achievement.Value);
        int balance = totalPoints - pointsUsed;

        if (balance < reward.Cost)
        {
            _logger.LogInformation("User does not have enough points to claim reward");
            return new ClaimRewardResult
            {
                status = RewardStatus.NotEnoughPoints
            };
        }

        try
        {
            // award the user an achievement for claiming their first prize
            if (!user.UserRewards.Any())
            {
                var achievement = await _context.Achievements
                    .TagWithContext("GetClaimPrizeAchievement")
                    .FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.ClaimPrize, cancellationToken);
                if (achievement != null)
                {
                    user.UserAchievements.Add(new UserAchievement { Achievement = achievement });
                }
            }

            user.UserRewards.Add(
                 new UserReward
                 {
                     UserId = user.Id,
                     RewardId = reward.Id,
                     AwardedAt = _dateTime.Now,
                 });

            if (pendingRedemption != null)
            {
                pendingRedemption.Completed = true;
            }

            await _context.SaveChangesAsync(cancellationToken);

            _cacheService.Remove(CacheTags.UpdatedOnlyRewards);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Unable to claim reward {RewardId} for {UserId} user", reward?.Id, userId);
            return new ClaimRewardResult
            {
                status = RewardStatus.Error
            };
        }

        var rewardModel = _mapper.Map<RewardDto>(reward);

        return new ClaimRewardResult
        {
            status = request.IsPendingRedemption ? RewardStatus.Confirmed : RewardStatus.Claimed,
            Reward = rewardModel
        };
    }
}