using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Reward.Queries.Common
{
    public class GetRewardListQuery : IRequest<RewardListViewModel>
    {
        public sealed class GetRewardListQueryHandler : IRequestHandler<GetRewardListQuery, RewardListViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public GetRewardListQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }

            public async Task<RewardListViewModel> Handle(GetRewardListQuery request, CancellationToken cancellationToken)
            {
                var rewards = await _context
                    .Rewards
                    .ProjectTo<RewardViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync(cancellationToken);

                return new RewardListViewModel
                {
                    Rewards = rewards
                };
            }
        }
    }
}
