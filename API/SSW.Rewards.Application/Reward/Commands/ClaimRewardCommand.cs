
using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using SSW.Rewards.Application.Reward.Queries.GetRewardList;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Reward.Commands
{
    public class ClaimRewardCommand : IRequest<ClaimRewardResult>
    {
        public string Code { get; set; }

        public class ClaimRewardCommandHandler : IRequestHandler<ClaimRewardCommand, ClaimRewardResult>
        {
            private readonly ICurrentUserService _currentUserService;
            private readonly ISSWRewardsDbContext _context;
            private readonly IMapper _mapper;
            private readonly IDateTimeProvider _dateTimeProvider;

            public ClaimRewardCommandHandler(
                ICurrentUserService currentUserService,
                ISSWRewardsDbContext context,
                IMapper mapper,
                IDateTimeProvider dateTimeProvider)
            {
                _currentUserService = currentUserService;
                _context = context;
                _mapper = mapper;
                _dateTimeProvider = dateTimeProvider;
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

                if(user.Points < reward.Cost)
                {
                    return new ClaimRewardResult
                    {
                        status = RewardStatus.NotEnoughPoints
                    };
                }

                var userHasReward = await _context
                    .UserRewards
                    .Where(ur => ur.UserId == user.Id)
                    .Where(ur => ur.RewardId == reward.Id)
                    .FirstOrDefaultAsync(cancellationToken);

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
