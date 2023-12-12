using AutoMapper.QueryableExtensions;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;

public class GetRewardAdminListQuery : IRequest<RewardAdminListViewModel>
{
    public sealed class GetRewardAdminListQueryHandler : IRequestHandler<GetRewardAdminListQuery, RewardAdminListViewModel>
    {
        private readonly IMapper _mapper;
        private readonly IApplicationDbContext _context;

        public GetRewardAdminListQueryHandler(
            IMapper mapper,
            IApplicationDbContext context)
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
