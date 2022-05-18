using AutoMapper;
using MediatR;
using Microsoft.EntityFrameworkCore;
using MoreLinq;
using SSW.Rewards.Application.Common.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Users.Queries.GetUserRewards
{
    public class GetUserRewardsQuery : IRequest<UserRewardsViewModel>
    {
        public int UserId { get; set; }

        public class GetUserRewardsQueryHandler : IRequestHandler<GetUserRewardsQuery, UserRewardsViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public GetUserRewardsQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<UserRewardsViewModel> Handle(GetUserRewardsQuery request, CancellationToken cancellationToken)
            {
                var rewards = await _context.Rewards.ToListAsync(cancellationToken);
                var userRewards = await _context.UserRewards
                    .Where(ur => ur.UserId == request.UserId)
                    .ToListAsync(cancellationToken);

                // Currently using in-memory join because the expected returned records are very low (max 10 or so)
                var vm = new List<UserRewardViewModel>();
                foreach (var reward in rewards)
                {
                    var userReward = userRewards.Where(ur => ur.RewardId == reward.Id).FirstOrDefault();
                    if (userReward != null)
                    {
                        vm.Add(_mapper.Map<UserRewardViewModel>(userReward));
                    }
                    else
                    {
                        vm.Add(new UserRewardViewModel
                        {
                            RewardName = reward.Name,
                            RewardCost = reward.Cost
                        });
                    }
                }

                return new UserRewardsViewModel
                {
                    UserId = request.UserId,
                    UserRewards = vm
                };
            }
        }
    }
}