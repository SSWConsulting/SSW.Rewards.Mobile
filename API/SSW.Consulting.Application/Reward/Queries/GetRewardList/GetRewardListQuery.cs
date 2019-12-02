using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Consulting.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Consulting.Application.Reward.Queries.GetRewardList
{
    public class GetRewardListQuery : IRequest<RewardListViewModel>
    {
        public sealed class GetRewardListQueryHandler : IRequestHandler<GetRewardListQuery, RewardListViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWConsultingDbContext _context;

            public GetRewardListQueryHandler(
                IMapper mapper,
                ISSWConsultingDbContext context)
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
