using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Rewards.Common;
using SSW.Rewards.Application.System.Commands.Common;
using SSW.Rewards.Domain.Entities;

namespace SSW.Rewards.Application.Rewards.Commands;

public class ClaimRewardCommand : IRequest<ClaimRewardResult>
{
    public string Code { get; set; }

    public bool ClaimInPerson { get; set; } = true;
}

public class ClaimRewardCommandHandler : IRequestHandler<ClaimRewardCommand, ClaimRewardResult>
{
    private readonly IUserService _userService;
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;
    private readonly IRewardSender _rewardSender;
    private readonly ICurrentUserService _currentUserService;

    public ClaimRewardCommandHandler(
        IUserService userService,
        IApplicationDbContext context,
        IMapper mapper,
        IRewardSender rewardSender,
        ICurrentUserService currentUserService)
    {
        _userService = userService;
        _context = context;
        _mapper = mapper;
        _rewardSender = rewardSender;
        _currentUserService = currentUserService;
    }

    public async Task<ClaimRewardResult> Handle(ClaimRewardCommand request, CancellationToken cancellationToken)
    {
        var reward = await _context.Rewards.FirstOrDefaultAsync(r => r.Code == request.Code, cancellationToken);

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

        if (!user.UserRewards.Any())
        {
            var achievement = await _context.Achievements.FirstOrDefaultAsync(a => a.Name == MilestoneAchievements.ClaimPrize, cancellationToken);
            user.UserAchievements.Add(new UserAchievement { Achievement = achievement });
        }

        user.UserRewards.Add(new UserReward
        {
            Reward = reward
        });

        await _context.SaveChangesAsync(cancellationToken);

        await _rewardSender.SendRewardAsync(user, reward, cancellationToken);

        var rewardModel = _mapper.Map<RewardViewModel>(reward);

        return new ClaimRewardResult
        {
            viewModel = rewardModel,
            status = RewardStatus.Claimed
        };
    }
}
