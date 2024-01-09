using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;

public class GetRewardAdminListQuery : IRequest<RewardsAdminViewModel> { }

public class GetRewardAdminListQueryHandler : IRequestHandler<GetRewardAdminListQuery, RewardsAdminViewModel>
{
    private readonly IApplicationDbContext _context;
    private readonly IMapper _mapper;

    public GetRewardAdminListQueryHandler(IApplicationDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<RewardsAdminViewModel> Handle(GetRewardAdminListQuery request, CancellationToken cancellationToken)
    {
        var rewards = await _context.Rewards
            .ProjectTo<RewardEditDto>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        var vm = new RewardsAdminViewModel
        {
            Rewards = rewards
        };

        return vm;
    }
}   