using AutoMapper.QueryableExtensions;
using SSW.Rewards.Application.Rewards.Common;

namespace SSW.Rewards.Application.Rewards.Queries.GetOnboardingRewards;
public class GetOnboardingRewards : IRequest<RewardListViewModel>
{
}

public class GetOnboardingRewardsHandler : IRequestHandler<GetOnboardingRewards, RewardListViewModel>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IMapper _mapper;

    public GetOnboardingRewardsHandler(IApplicationDbContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<RewardListViewModel> Handle(GetOnboardingRewards request, CancellationToken cancellationToken)
    {
        var rewards = await _dbContext.Rewards.Where(r => r.IsOnboardingReward)
            .ProjectTo<RewardViewModel>(_mapper.ConfigurationProvider)
            .ToListAsync(cancellationToken);

        return new RewardListViewModel { Rewards = rewards };
    }
}
