using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.Shared.DTOs.AddressTypes;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Commands.ClaimReward;

public class ClaimRewardCommand : IRequest<ClaimRewardResult>
{
    public string Code { get; set; }
    public int Id { get; set; }

    public Address? Address { get; set; }
    public bool ClaimInPerson { get; set; }
}

public class ClaimRewardCommandHandler : IRequestHandler<ClaimRewardCommand, ClaimRewardResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IRewardSender _rewardSender;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ClaimRewardCommandHandler> _logger;

    public ClaimRewardCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IRewardSender rewardSender,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        ILogger<ClaimRewardCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _rewardSender = rewardSender;
        _currentUserService = currentUserService;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<ClaimRewardResult> Handle(ClaimRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards
            .AsNoTracking()
            .TagWithContext("GetReward")
            .FirstOrDefaultAsync(r => r.Code == request.Code || r.Id == request.Id, cancellationToken);

        if (reward == null)
        {
            _logger.LogWarning("Reward with code {Code} or ID {Id} not found.", request.Code, request.Id);
            return new ClaimRewardResult
            {
                status = RewardStatus.NotFound
            };
        }

        int claimPrizeAchievementId = await _cacheService.GetOrAddAsync(
            CacheKeys.ClaimPrizeAchievementId,
            () => GetClaimPrizeAchievementId(cancellationToken));

        var userAndPoints = await _context.Users
            .AsNoTracking()
            .TagWithContext("GetUser")
            .Where(u => u.Email == _currentUserService.GetUserEmail())
            .Select(x => new
            {
                User = x,
                UserId = x.Id,
                PointsUsed = x.UserRewards.Sum(ur => ur.Reward.Cost),
                TotalPoints = x.UserAchievements.Sum(ua => ua.Achievement.Value),
                HasClaimedPrizeAchievement = x.UserAchievements.Any(ua => ua.AchievementId == claimPrizeAchievementId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (userAndPoints == null)
        {
            _logger.LogWarning("User with email {Email} not found.", _currentUserService.GetUserEmail());
            return new ClaimRewardResult
            {
                status = RewardStatus.Error
            };
        }

        int balance = userAndPoints.TotalPoints - userAndPoints.PointsUsed;
        if (balance < reward.Cost)
        {
            return new ClaimRewardResult
            {
                status = RewardStatus.NotEnoughPoints
            };
        }

        _context.UserRewards.Add(new()
        {
            UserId = userAndPoints.UserId,
            RewardId = reward.Id
        });

        // Award the user an achievement for claiming their first prize.
        if (!userAndPoints.HasClaimedPrizeAchievement && claimPrizeAchievementId != -1)
        {
            _context.UserAchievements.Add(new UserAchievement
            {
                UserId = userAndPoints.UserId,
                AchievementId = claimPrizeAchievementId
            });
        }

        await _context.SaveChangesAsync(cancellationToken);

        _cacheService.Remove(CacheTags.UpdatedOnlyRewards);

        if (!request.ClaimInPerson)
        {
            await _rewardSender.SendRewardAsync(userAndPoints.User, reward, request.Address?.freeformAddress ?? string.Empty, cancellationToken);
        }

        var rewardModel = _mapper.Map<RewardDto>(reward);

        return new ClaimRewardResult
        {
            Reward = rewardModel,
            status = RewardStatus.Claimed
        };
    }

    private async Task<int> GetClaimPrizeAchievementId(CancellationToken ct)
    {
        var achievement = await _context.Achievements
            .AsNoTracking()
            .TagWithContext("GetClaimPrizeAchievement")
            .Where(a => a.Name == MilestoneAchievements.ClaimPrize)
            .Select(x => new { x.Id })
            .FirstOrDefaultAsync(ct);

        return achievement?.Id ?? -1;
    }
}
