using Hangfire;
using Microsoft.Extensions.Logging;
using SSW.Rewards.Application.Rewards.BackgroundJobs;
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
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ICurrentUserService _currentUserService;
    private readonly ICacheService _cacheService;
    private readonly ILogger<ClaimRewardCommandHandler> _logger;

    public ClaimRewardCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IBackgroundJobClient backgroundJobClient,
        ICurrentUserService currentUserService,
        ICacheService cacheService,
        ILogger<ClaimRewardCommandHandler> logger)
    {
        _context = context;
        _mapper = mapper;
        _backgroundJobClient = backgroundJobClient;
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

        var userReward = new UserReward
        {
            UserId = userAndPoints.UserId,
            RewardId = reward.Id
        };
        _context.UserRewards.Add(userReward);

        // Outbox: the fulfilment request (and shipping address) is saved in the same transaction as the claim,
        // so a failed email can be retried - and an interrupted delivery resumed - from the database.
        RewardClaimDelivery? delivery = null;
        if (!request.ClaimInPerson)
        {
            delivery = CreateDelivery(userReward, request.Address);
            _context.RewardClaimDeliveries.Add(delivery);
        }

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

        if (delivery != null)
        {
            TryEnqueueDelivery(delivery);
        }

        var rewardModel = _mapper.Map<RewardDto>(reward);

        return new ClaimRewardResult
        {
            Reward = rewardModel,
            status = RewardStatus.Claimed
        };
    }

    private static RewardClaimDelivery CreateDelivery(UserReward userReward, Address? address) => new()
    {
        UserReward = userReward,
        FreeformAddress = address?.freeformAddress,
        StreetNumber = address?.streetNumber,
        StreetName = address?.streetName,
        Municipality = address?.municipality,
        State = address?.countrySubdivisionName ?? address?.countrySubdivision,
        PostalCode = address?.postalCode,
        Country = address?.country,
        Status = RewardClaimDeliveryStatus.Pending
    };

    private void TryEnqueueDelivery(RewardClaimDelivery delivery)
    {
        // The claim is already saved: never fail the request because of the email.
        // If the enqueue fails, RewardClaimDeliverySweepJob picks the Pending record up later.
        try
        {
            RewardClaimDeliveryJob.Enqueue(_backgroundJobClient, delivery.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to enqueue reward claim delivery {DeliveryId} for claim #{ClaimId}; the sweep job will retry it",
                delivery.Id, delivery.UserRewardId);
        }
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
