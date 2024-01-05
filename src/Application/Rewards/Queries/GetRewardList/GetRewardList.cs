using AutoMapper.QueryableExtensions;
using SSW.Rewards.Shared.DTOs.Rewards;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardList;
public class GetRewardList : IRequest<RewardListViewModel> { }

public sealed class GetRewardListHandler : IRequestHandler<GetRewardList, RewardListViewModel>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public GetRewardListHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<RewardListViewModel> Handle(GetRewardList request, CancellationToken cancellationToken)
    {
        var rewards = await _context
                .Rewards
                .ProjectTo<RewardDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

        return new RewardListViewModel
        {
            Rewards = rewards
        };
    }
}
