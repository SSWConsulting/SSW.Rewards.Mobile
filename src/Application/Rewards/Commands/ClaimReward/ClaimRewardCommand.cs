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

    public ClaimRewardCommandHandler(
        IApplicationDbContext context,
        IMapper mapper,
        IRewardSender rewardSender,
        ICurrentUserService currentUserService)
    {
        _context = context;
        _mapper = mapper;
        _rewardSender = rewardSender;
        _currentUserService = currentUserService;
    }

    public async Task<ClaimRewardResult> Handle(ClaimRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.Code == request.Code || r.Id == request.Id, cancellationToken);

        if (reward == null)
        {
            return new ClaimRewardResult
            {
                status = RewardStatus.NotFound
            };
        }

        var user = await _context.Users
            .Where(u => u.Email == _currentUserService.GetUserEmail())
            .Include(u => u.UserRewards)
                .ThenInclude(ur => ur.Reward)
            .Include(u => u.UserAchievements)
                .ThenInclude(u => u.Achievement)
            .FirstOrDefaultAsync(cancellationToken);

        int pointsUsed = user.UserRewards.Sum(ur => ur.Reward.Cost);

        int totalPoints = user.UserAchievements.Sum(ua => ua.Achievement.Value);

        int balance = totalPoints - pointsUsed;

        if (balance < reward.Cost)
        {
            return new ClaimRewardResult
            {
                status = RewardStatus.NotEnoughPoints
            };
        }

        // award the user an achievement for claiming their first prize
        if (!user.UserRewards.Any())
        {
            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.ClaimPrize, cancellationToken);
            if (achievement != null)
            {
                user.UserAchievements.Add(new UserAchievement { Achievement = achievement });
            }
        }

        user.UserRewards.Add(new UserReward
        {
            Reward = reward
        });

        await _context.SaveChangesAsync(cancellationToken);

        if (!request.ClaimInPerson)
        {
            await _rewardSender.SendRewardAsync(user, reward, request.Address?.freeformAddress??string.Empty, cancellationToken);
        }

        var rewardModel = _mapper.Map<RewardDto>(reward);

        return new ClaimRewardResult
        {
            Reward = rewardModel,
            status = RewardStatus.Claimed
        };
    }
}
