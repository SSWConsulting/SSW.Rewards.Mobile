using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Rewards.Common;

namespace SSW.Rewards.Application.Rewards.Queries.Common;

public class GetRewardListQuery : IRequest<RewardListViewModel>
{
    public sealed class GetRewardListQueryHandler : IRequestHandler<GetRewardListQuery, RewardListViewModel>
    {
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        public GetRewardListQueryHandler(
            IMapper mapper,
            IApplicationDbContext context)
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
