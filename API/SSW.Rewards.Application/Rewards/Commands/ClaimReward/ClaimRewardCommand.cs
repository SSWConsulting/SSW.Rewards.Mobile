
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Rewards.Queries.Common;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Rewards.Commands
{
    public class ClaimRewardCommand : IRequest<ClaimRewardResult>
    {
        public string Code { get; set; }

        public bool ClaimInPerson { get; set; } = true;

        public class ClaimRewardCommandHandler : IRequestHandler<ClaimRewardCommand, ClaimRewardResult>
        {
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWRewardsDbContext _context;
            private readonly IMapper _mapper;
            private readonly IDateTimeProvider _dateTimeProvider;
            private readonly IRewardSender _rewardSender;

            public ClaimRewardCommandHandler(
                ICurrentUserService currentUserService,
                ISSWRewardsDbContext context,
                IMapper mapper,
                IDateTimeProvider dateTimeProvider,
                IRewardSender rewardSender)
            {
                _currentUserService = currentUserService;
                _context = context;
                _mapper = mapper;
                _dateTimeProvider = dateTimeProvider;
                _rewardSender = rewardSender;
            }

            public async Task<ClaimRewardResult> Handle(ClaimRewardCommand request, CancellationToken cancellationToken)
            {
                var reward = await _context
                    .Rewards
                    .Where(r => r.Code == request.Code)
                    .FirstOrDefaultAsync(cancellationToken);

                if(reward == null)
                {
                    return new ClaimRewardResult
                    {
                        status = RewardStatus.NotFound
                    };
                }

                var user = await _currentUserService.GetCurrentUserAsync(cancellationToken);

                var userRewards = await _context
                    .UserRewards
                    .Where(ur => ur.UserId == user.Id)
                    .ToListAsync(cancellationToken);

                int pointsUsed = userRewards.Sum(ur => ur.Reward.Cost);

                int balance = user.Points - pointsUsed;

                if(balance < reward.Cost)
                {
                    return new ClaimRewardResult
                    {
                        status = RewardStatus.NotEnoughPoints
                    };
                }

                // TECH DEBT: the following logic is intended to 'debounce' reward
                // claiming, to prevent users claiming the same reward twice
                // within a 5 minute window. This workaround is only required on 
                // the current 'milestone' model for points. Once we move to the
                // 'currency' model, this will not be required anymore.
                // see: https://github.com/SSWConsulting/SSW.Rewards/issues/100
                var userHasReward = userRewards
                    .Where(ur => ur.RewardId == reward.Id)
                    .FirstOrDefault();

                if(userHasReward != null && userHasReward.AwardedAt >= _dateTimeProvider.Now.AddMinutes(-5))
                {
                    return new ClaimRewardResult
                    {
                        status = RewardStatus.Duplicate
                    };
                }

                await _context
                    .UserRewards
                    .AddAsync(new Domain.Entities.UserReward
                    {
                        UserId = user.Id,
                        RewardId = reward.Id
                    }, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);

                var dbUser = await _context.Users
                    .Where(u => u.Id == user.Id)
                    .FirstAsync(cancellationToken);

                await _rewardSender.SendRewardAsync(dbUser, reward, cancellationToken);

                var rewardModel = _mapper.Map<RewardViewModel>(reward);

                return new ClaimRewardResult
                {
                    viewModel = rewardModel,
                    status = RewardStatus.Claimed
                };
            }
        }
    }
}
