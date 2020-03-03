using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;
using System.Threading;
using System.Threading.Tasks;

namespace SSW.Rewards.Application.Reward.Queries.GetRewardAdminList
{
    public class GetRewardAdminListQuery : IRequest<RewardAdminListViewModel>
    {
        public sealed class GetRewardAdminListQueryHandler : IRequestHandler<GetRewardAdminListQuery, RewardAdminListViewModel>
        {
            private readonly IMapper _mapper;
            private readonly ISSWRewardsDbContext _context;

            public GetRewardAdminListQueryHandler(
                IMapper mapper,
                ISSWRewardsDbContext context)
            {
                _mapper = mapper;
                _context = context;
            }
            public async Task<RewardAdminListViewModel> Handle(GetRewardAdminListQuery request, CancellationToken cancellationToken)
            {
                var result = await _context
                    .Rewards
                    .ProjectTo<RewardAdminViewModel>(_mapper.ConfigurationProvider)
                    .ToListAsync();

                return new RewardAdminListViewModel
                {
                    Rewards = result
                };
            }
        }
    }
}
