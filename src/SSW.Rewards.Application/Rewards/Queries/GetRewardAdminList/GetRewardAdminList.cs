using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Rewards.Queries.GetRewardAdminList;
public class GetRewardAdminList : IRequest<RewardAdminListViewModel>
{
    
}

public sealed class GetRewardAdminListHandler : IRequestHandler<GetRewardAdminList, RewardAdminListViewModel>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public GetRewardAdminListHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper     = mapper;
        _context    = context;
    }

    public async Task<RewardAdminListViewModel> Handle(GetRewardAdminList request, CancellationToken cancellationToken)
    {
        var result = await _context
                .Rewards
                .ProjectTo<RewardAdminDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

        return new RewardAdminListViewModel
        {
            Rewards = result
        };
    }
}