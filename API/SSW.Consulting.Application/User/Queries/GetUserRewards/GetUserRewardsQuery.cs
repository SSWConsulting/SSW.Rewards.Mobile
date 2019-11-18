using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.User.Queries.GetUserRewards
{
    public class GetUserRewardsQuery : IRequest<UserRewardsViewModel>
    {
        public int UserId { get; set; }

        public class GetUserRewardsQueryHandler : IRequestHandler<GetUserRewardsQuery, UserRewardsViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWConsultingDbContext _context;

            public GetUserRewardsQueryHandler(
                IMapper mapper,
                ISSWConsultingDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<UserRewardsViewModel> Handle(GetUserRewardsQuery request, CancellationToken cancellationToken)
            {
                var userRewards = await _context.Rewards
                    .Include(r => r.UserRewards)
                    .Select(r => new JoinedUserReward
                    {
                        Reward = r,
                        UserReward = r.UserRewards.FirstOrDefault(ur => ur.UserId == request.UserId)
                    })
                    .ProjectTo<UserRewardViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new UserRewardsViewModel
                {
                    UserId = request.UserId,
                    UserRewards = userRewards
                };
            }
        }
    }
}
