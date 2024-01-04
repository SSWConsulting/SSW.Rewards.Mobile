using Microsoft.Extensions.Logging;
using Shared.DTOs.Rewards;
using SSW.Rewards.Application.System.Commands.Common;

namespace SSW.Rewards.Application.Rewards.Commands;

public class ClaimRewardForUserCommand : IRequest<ClaimRewardResult>
{
    public int UserId { get; set; }
    public string Code { get; set; }
}

public class ClaimRewardForUserCommandHandler : IRequestHandler<ClaimRewardForUserCommand, ClaimRewardResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly ILogger<ClaimRewardForUserCommandHandler> _logger;
    private readonly IDateTime _dateTime;

    public ClaimRewardForUserCommandHandler(
            IApplicationDbContext context,
            IMapper mapper,
            ILogger<ClaimRewardForUserCommandHandler> logger,
            IDateTime dateTime)
    {
        _context = context;
        _mapper = mapper;
        _logger = logger;
        _dateTime = dateTime;
    }

    public async Task<ClaimRewardResult> Handle(ClaimRewardForUserCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context
            .Rewards
            .Where(r => r.Code == request.Code)
            .FirstOrDefaultAsync(cancellationToken);

        if (reward == null)
        {
            _logger.LogError("Reward not found with code: {0}", request.Code);
            return new ClaimRewardResult
            {
                status = RewardStatus.NotFound
            };
        }

        User? user = await _context.Users
            .Where(u => u.Id == request.UserId)
            .Include(u => u.UserAchievements).ThenInclude(ua => ua.Achievement)
            .FirstOrDefaultAsync(cancellationToken);

        if (user == null)
        {
            _logger.LogError("User with ID {userId} claiming the reward {rewardId}({rewardName}) does not exist!", request.UserId, reward.Id, reward.Name);
            return new ClaimRewardResult
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
                var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.ClaimPrize, cancellationToken);
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

            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError(e.Message, e);
            return new ClaimRewardResult
            {
                status = RewardStatus.Error
            };
        }

        var rewardModel = _mapper.Map<RewardDto>(reward);

        return new ClaimRewardResult
        {
            status = RewardStatus.Claimed,
            Reward = rewardModel
        };
    }
}