using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Reward.Queries.GetRecentRewards
{
    public class GetRecentRewardsQueryHandler : IRequestHandler<GetRecentRewardsQuery, RecentRewardListViewModel>
    {
        private IMapper _mapper;
        private ISSWRewardsDbContext _context;

        public GetRecentRewardsQueryHandler(
            IMapper mapper,
            ISSWRewardsDbContext context)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<RecentRewardListViewModel> Handle(GetRecentRewardsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.UserRewards.Where(u => u.Id != 0);

            if(!(request.Since == null))
            {
                query = query
                    .Where(u => u.AwardedAt > request.Since.Value.ToUniversalTime());
            }
            else
            {
                query = query
                    .OrderByDescending(u => u.AwardedAt)
                    .Take(10);
            }

            var results = await query
                .ProjectTo<RecentRewardViewModel>(_mapper.ConfigurationProvider)
                .ToListAsync();

            return new RecentRewardListViewModel
            {
                Rewards = results
            };
        }
    }
}
