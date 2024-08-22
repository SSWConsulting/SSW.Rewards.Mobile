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
                // TODO: Remove this once Copenhagen Developers Festival is over. 
                .OrderByDescending(x => x.Name.Contains("Tina"))
                .ToListAsync(cancellationToken);

        return new RewardListViewModel
        {
            Rewards = rewards
        };
    }
}
