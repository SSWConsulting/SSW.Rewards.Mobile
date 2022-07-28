using AutoMapper;
using AutoMapper.QueryableExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using SSW.Rewards.Application.Common.Interfaces;

namespace SSW.Rewards.Application.Rewards.Queries.GetRecentRewards;
public class GetRecentRewards : IRequest<RecentRewardListViewModel>
{
    public DateTime? Since { get; set; }

}

public sealed class GetRecentRewardsHandler : IRequestHandler<GetRecentRewards, RecentRewardListViewModel>
{
    private readonly IMapper _mapper;
    private readonly IApplicationDbContext _context;

    public GetRecentRewardsHandler(
        IMapper mapper,
        IApplicationDbContext context)
    {
        _mapper = mapper;
        _context = context;
    }

    public async Task<RecentRewardListViewModel> Handle(GetRecentRewards request, CancellationToken cancellationToken)
    {
        var query = _context.UserRewards.Where(u => u.Id != 0);

        if (!(request.Since == null))
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
            .ProjectTo<RecentRewardDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return new RecentRewardListViewModel
        {
            Rewards = results
        };
    }
}
